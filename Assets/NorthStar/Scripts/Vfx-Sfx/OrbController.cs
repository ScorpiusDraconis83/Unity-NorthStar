// Copyright (c) Meta Platforms, Inc. and affiliates.
using System;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace NorthStar
{
    /// <summary>
    /// Controller for the mysterious silver orb found in Beat 7
    /// </summary>
    public class OrbController : MonoBehaviour
    {
        private static readonly int s_leftHandPositionProperty = Shader.PropertyToID("_LeftHandPosition");
        private static readonly int s_rightHandPositionProperty = Shader.PropertyToID("_RightHandPosition");
        private static readonly int s_leftHandSurfacePosProperty = Shader.PropertyToID("_LeftHandSurfacePosition");
        private static readonly int s_rightHandSurfacePosProperty = Shader.PropertyToID("_RightHandSurfacePosition");
        private static readonly int s_uvOffsetProperty = Shader.PropertyToID("_PatternOffset");

        private static readonly int s_jointPosX = Shader.PropertyToID("_JointPosX");
        private static readonly int s_jointPosY = Shader.PropertyToID("_JointPosY");
        private static readonly int s_jointPosZ = Shader.PropertyToID("_JointPosZ");

        [Header("Orb Components")]
        [SerializeField] private Transform m_orbTransform;
        [SerializeField] private Transform m_leftHandTransform;
        [SerializeField] private Transform m_rightHandTransform;
        [SerializeField] private ActiveStateGroup m_leftHandGrabbed, m_rightHandGrabbed;
        [SerializeField] private float m_orbOffsetDistance = .2f;
        [SerializeField, Range(-1, 1)] private float m_handDirCutoff = -.5f;
        [SerializeField] private Renderer[] m_orbRenderers;
        [SerializeField] private bool m_movementEnabled;

        [Header("Transform UVs to simulate rotation")]
        [SerializeField] private bool m_useRotationUVOffset;
        [SerializeField] private float m_offsetFactor;

        [Header("Camera Facing Rotation")]
        [SerializeField] private bool m_cameraFacing;
        [SerializeField] private Transform m_cameraFacingTransform;
        [SerializeField] private float m_rotationSpeed = 1f;
        [SerializeField] private float m_screenAimFactor = 0.1f;

        [Header("SpringJoints")]
        [SerializeField] private GameObject[] m_springJoints;

        private Camera m_camera;
        private Vector2 m_currentUVOffset;
        private Quaternion m_lastCameraRotation;

        private Vector3[] m_jointInitialPositions;

        [Header("Movement")]
        [Tooltip("How much force should be applied to the orb in the opposite direction of the hand based on it's proximity (meters) to the orb.")]
        [SerializeField] private AnimationCurve m_handForce;

        [SerializeField] private float m_drag;
        [SerializeField] private Vector3 m_velocity;
        private Vector3 m_originalPos;
        private MaterialPropertyBlock m_orbProperties;

        public UnityEvent OnGrabbed;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            StoreJointPositions();
            DrawHandGizmos();
            DrawJointGizmos();
        }

        private void DrawHandGizmos()
        {
            var scale = gameObject.transform.localScale.x;
            Gizmos.color = Color.green;
            Gizmos.DrawRay(m_orbTransform.position, CalculateDirection(m_leftHandTransform.position, m_orbTransform.position) * scale);
            Gizmos.DrawWireSphere(CalculateSphereIntersection(m_leftHandTransform.position, m_orbTransform.position, scale), 0.01f);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(m_orbTransform.position, CalculateDirection(m_rightHandTransform.position, m_orbTransform.position) * scale);
            Gizmos.DrawWireSphere(CalculateSphereIntersection(m_rightHandTransform.position, m_orbTransform.position, scale), 0.01f);
        }

        private void DrawJointGizmos()
        {
            Gizmos.color = Color.blue;
            for (var i = 0; i < m_jointInitialPositions.Length; i++)
            {
                var initialPosition = m_jointInitialPositions[i];
                Gizmos.DrawSphere(transform.TransformPoint(initialPosition), .05f);
                Gizmos.DrawRay(transform.TransformPoint(initialPosition), m_springJoints[0].transform.position - transform.TransformPoint(initialPosition));
            }
        }
#endif
        public void EnableMovement(bool enable)
        {
            m_movementEnabled = enable;
        }
        private void Start()
        {
            m_orbProperties = new();
            m_originalPos = transform.position;

            m_camera = Camera.main;
            UnparentSpringJoints();
            StoreJointPositions();
        }

        private void Update()
        {
            UpdateMaterialParameters();
            if (m_cameraFacing) CameraLookAt(m_camera, m_cameraFacingTransform, m_rotationSpeed);
            if (m_useRotationUVOffset) UpdateMaterialUVOffset();


            var targetPosition = Vector3.zero;
            var hands = 0;
            if (!m_leftHandGrabbed.Active && m_movementEnabled && Vector3.Dot(-m_leftHandTransform.up, m_camera.transform.forward) > m_handDirCutoff)
            {
                targetPosition += m_leftHandTransform.position - m_leftHandTransform.up * m_orbOffsetDistance;
                hands++;
            }
            if (!m_rightHandGrabbed.Active && m_movementEnabled && Vector3.Dot(-m_rightHandTransform.up, m_camera.transform.forward) > m_handDirCutoff)
            {
                targetPosition += m_rightHandTransform.position - m_rightHandTransform.up * m_orbOffsetDistance;
                hands++;
            }

            if (hands > 0)
            {
                targetPosition /= hands;
            }
            else
            {
                targetPosition = m_originalPos;
            }

            var toTarget = targetPosition - transform.position;            
            var force = toTarget.normalized * m_handForce.Evaluate(toTarget.magnitude);

            m_velocity += force * Time.deltaTime;

            //Apply velocity
            transform.position += m_velocity * Time.deltaTime;

            //Apply Drag
            var multiplier = 1.0f - m_drag * Time.deltaTime;
            if (multiplier < 0.0f) multiplier = 0.0f;
            m_velocity *= multiplier;

            UpdateSprintJointPositions();

            AssignMaterialProperties();
        }

        private Vector3 CalculateDirection(Vector3 from, Vector3 to)
        {
            return (from - to).normalized;
        }

        private Vector3 CalculateSphereIntersection(Vector3 target, Vector3 source, float scale)
        {
            return source + CalculateDirection(target, source) * scale;
        }

        private void UpdateMaterialParameters()
        {
            var orbSize = gameObject.transform.localScale.x;
            m_orbProperties.SetVector(s_leftHandPositionProperty, m_leftHandTransform.position);
            m_orbProperties.SetVector(s_rightHandPositionProperty, m_rightHandTransform.position);
            m_orbProperties.SetVector(s_leftHandSurfacePosProperty, CalculateSphereIntersection(m_leftHandTransform.position, m_orbTransform.position, orbSize));
            m_orbProperties.SetVector(s_rightHandSurfacePosProperty, CalculateSphereIntersection(m_rightHandTransform.position, m_orbTransform.position, orbSize));
        }

        private void CameraLookAt(Camera camera, Transform transform, float rotationSpeed)
        {
            transform.rotation = Quaternion.LookRotation(m_orbTransform.position - camera.transform.position, transform.up);
        }

        private void UpdateMaterialUVOffset()
        {
            var currentRotation = m_cameraFacingTransform.rotation;
            var deltaRotation = Quaternion.Inverse(m_lastCameraRotation) * currentRotation;
            var deltaYaw = Mathf.DeltaAngle(0, deltaRotation.eulerAngles.y);
            var deltaPitch = Mathf.DeltaAngle(0, deltaRotation.eulerAngles.x);
            var uvOffsetChange = new Vector2(-deltaYaw * m_offsetFactor, deltaPitch * m_offsetFactor);
            m_currentUVOffset += uvOffsetChange;
            m_orbProperties.SetVector(s_uvOffsetProperty, m_currentUVOffset);
            m_lastCameraRotation = currentRotation;
        }

        private void UnparentSpringJoints()
        {
            foreach (var joint in m_springJoints)
            {
                joint.transform.parent = null;
            }
        }

        private void StoreJointPositions()
        {
            m_jointInitialPositions = new Vector3[m_springJoints.Length];
            for (var i = 0; i < m_springJoints.Length; i++)
            {
                m_jointInitialPositions[i] = transform.InverseTransformPoint(m_springJoints[i].transform.position);
            }
        }

        private void UpdateSprintJointPositions()
        {
            //Find the average position
            Span<Vector3> offsets = stackalloc Vector3[m_springJoints.Length];
            var average = Vector3.zero;
            for (var i = 0; i < m_springJoints.Length; i++)
            {
                offsets[i] = m_cameraFacingTransform.InverseTransformDirection(m_springJoints[i].transform.position - transform.TransformPoint(m_jointInitialPositions[i]));
                average += offsets[i];
            }
            average /= m_springJoints.Length;

            //Set the new orb positions and offset them using the average
            for (var i = 0; i < m_springJoints.Length; i++)
            {
                m_orbProperties.SetVector(s_jointPosX, offsets[i] - average * .8f);
            }
        }

        private void AssignMaterialProperties()
        {
            foreach (var renderer in m_orbRenderers)
            {
                renderer.SetPropertyBlock(m_orbProperties);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //other object has the player layer
            if (other.gameObject.layer == 3)
            {
                OnGrabbed.Invoke();
            }
        }
    }
}