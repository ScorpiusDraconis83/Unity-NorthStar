// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using Meta.Utilities.Environment;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Manages the sail logic and visual effects based on the current wind direction and sail angle
    /// </summary>
    public class SailAngleController : MonoBehaviour
    {
        private static readonly int s_sailDirection = Shader.PropertyToID("_SailDirection");
        private static readonly int s_height = Shader.PropertyToID("_Height");
        private static readonly int s_billow = Shader.PropertyToID("_Billow");
        private static readonly int s_billowLength = Shader.PropertyToID("_BillowLength");
        private static readonly int s_topBottomDistance = Shader.PropertyToID("_TopBottomDistance");

        [SerializeField] private bool m_invertDirection;
        [SerializeField] private bool m_boomMode = false;
        [SerializeField] private AnimationCurve m_boomHeightEasing;
        [SerializeField, Range(0, 1)] private float m_sailProgress = 0;
        [Range(0, 1)] public float RopeTightPercentage = 0;
        [Range(0, 1)] public float BoomSwingPoint = 0.1f;
        [SerializeField] private float m_minAngle = 10, m_maxAngle = 75;
        [SerializeField] private Transform m_target, m_topBoom, m_topBoomBrace, m_bottomBoom;
        [SerializeField] private float m_swingSpeed = 10;
        [SerializeField] private BoatController m_boat;
        [SerializeField] private SkinnedMeshRenderer m_sailRenderer;
        [SerializeField] private float m_topBoomMinHeight, m_topBoomMaxHeight;
        [SerializeField] private float m_topBottomDistance;
        [SerializeField] private float m_maxSailBillowLength = 4;
        private Material m_sailMaterial;
        private Quaternion m_startLocalRotation;
        private Quaternion m_currentRotation;

        // Right relative to the ship
        public bool WindToStarboard;

        private void Start()
        {
            m_startLocalRotation = m_target.localRotation;
            m_currentRotation = Quaternion.identity;
            m_sailMaterial = m_sailRenderer.material;
            m_sailMaterial.SetFloat("_TimeOffset", Random.value);
            m_boat = BoatController.Instance;
        }

        private void Update()
        {
            if (m_boomMode)
                WindToStarboard = Vector3.Dot(m_boat.MovementSource.Right, EnvironmentSystem.Instance.WindVector) > BoomSwingPoint;

            m_currentRotation = Quaternion.Slerp(m_currentRotation, Quaternion.Euler(0, GetTargetRotation(), 0), m_swingSpeed * Time.deltaTime);
            m_target.localRotation = m_startLocalRotation * m_currentRotation;

            if (m_bottomBoom != null)
                m_bottomBoom.localRotation = m_target.localRotation;

            if (m_topBoomBrace != null)
            {
                m_topBoomBrace.localPosition = new Vector3(
                 m_topBoomBrace.localPosition.x,
                 Mathf.Lerp(m_topBoomMinHeight, m_topBoomMaxHeight, m_boomHeightEasing.Evaluate(m_sailProgress)),
                  m_topBoomBrace.localPosition.z
                );
            }

            m_sailRenderer.transform.parent.rotation = m_target.rotation;

            const float THIRD = 1f / 3f;
            var sailShapeProgress = 1 - m_sailProgress;
            if (sailShapeProgress < THIRD)
            {
                var progress = sailShapeProgress.Map(0, THIRD, 0, 1);
                m_sailRenderer.SetBlendShapeWeight(2, progress * 100);
                m_sailRenderer.SetBlendShapeWeight(1, 0);
                m_sailRenderer.SetBlendShapeWeight(0, 0);
            }
            else if (sailShapeProgress < 2 * THIRD)
            {
                var progress = sailShapeProgress.Map(THIRD, 2 * THIRD, 0, 1);
                m_sailRenderer.SetBlendShapeWeight(2, (1 - progress) * 100);
                m_sailRenderer.SetBlendShapeWeight(1, progress * 100);
                m_sailRenderer.SetBlendShapeWeight(0, 0);
            }
            else
            {
                var progress = sailShapeProgress.Map(2 * THIRD, 1, 0, 1);
                m_sailRenderer.SetBlendShapeWeight(2, 0);
                m_sailRenderer.SetBlendShapeWeight(1, (1 - progress) * 100);
                m_sailRenderer.SetBlendShapeWeight(0, progress * 100);
            }

            if (m_boat is not null)
            {
                var sailDirection = GetSailDirection();
                var windVector = EnvironmentSystem.Instance.WindVector.normalized;
                var billowAmount = Vector3.Dot(windVector, sailDirection);
                SetMaterialValues(billowAmount);
            }
            else
            {
                SetMaterialValues(0);
            }
        }

        private float GetTargetRotation()
        {
            // Rope tightening works as a limit (when in boom mode) and assumes there is always wind present to swing the sail back out
            return Mathf.Lerp(m_maxAngle, m_minAngle, RopeTightPercentage) * (m_boomMode ? (WindToStarboard ? -1 : 1) : 1);
        }

        private void SetMaterialValues(float billowAmount)
        {
            if (m_boomMode)
                m_sailMaterial.SetFloat(s_sailDirection, WindToStarboard ? -1 : 1);
            else
                m_sailMaterial.SetFloat(s_sailDirection, 1);

            m_sailMaterial.SetFloat(s_height, Mathf.Clamp(m_sailProgress, .33f, 1));
            m_sailMaterial.SetFloat(s_billow, Mathf.Lerp(0, EasingFunction(billowAmount), m_sailProgress));
            m_sailMaterial.SetFloat(s_billowLength, Mathf.Lerp(1, m_maxSailBillowLength, m_sailProgress));
            m_sailMaterial.SetVector(s_topBottomDistance, new Vector4(m_topBottomDistance, 0, 0, 0));
        }

        public void SetSailHeight(float progress)
        {
            m_sailProgress = progress;
        }

        public void SetSailTightness(float progress)
        {
            RopeTightPercentage = Mathf.Clamp01(Mathf.Abs(progress));
        }

        public Vector3 GetSailDirection()
        {
            return Quaternion.Euler(0, m_boat.HeadingAngle, 0) * ((m_boomMode ? (m_target.right * (WindToStarboard ? 1 : -1)) : m_target.forward) * (m_invertDirection ? -1 : 1));
        }

        public float GetSailSpeed()
        {
            var sailDirection = GetSailDirection();
            var windVector = EnvironmentSystem.Instance.WindVector;
            return Mathf.Max(0, Vector3.Dot(windVector, sailDirection)) * m_sailProgress;
        }

        private float EasingFunction(float t)
        {
            return 1 - Mathf.Sqrt(1 - Mathf.Pow(t, 2));
        }
    }
}
