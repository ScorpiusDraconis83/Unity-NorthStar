// Copyright (c) Meta Platforms, Inc. and affiliates.
using System;
using Meta.Utilities;
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

        [SerializeField] private ParticleSystem m_leftHandParticleSystem;
        [SerializeField] private ParticleSystem m_rightHandParticleSystem;
        [SerializeField, Range(0f, 1f)] private float m_minForcefieldMultiplier = 0f;
        [SerializeField, Range(0f, 1f)] private float m_maxForcefieldMultiplier = 0.5f;
        [SerializeField] private float m_forceScaling = .1f;
        [SerializeField] private float m_orbMass = 1f;
        [Tooltip("How long in seconds it takes to go to max force")]
        [SerializeField, Range(0.01f, 1f)] private float m_forcefieldMultiplierRampUpTime = 0.65f;

        [Header("Orb Components")]
        [SerializeField] private Transform m_orbTransform;
        [SerializeField] private Transform m_leftHandTransform;
        [SerializeField] private Transform m_rightHandTransform;
        [SerializeField] private Rigidbody m_leftHandRigidbody;
        [SerializeField] private Rigidbody m_rightHandRigidbody;
        [SerializeField] private ActiveStateGroup m_leftHandGrabbed, m_rightHandGrabbed;
        [SerializeField] private float m_orbOffsetDistance = .2f;
        [SerializeField, Range(-1, 1)] private float m_handDirCutoff = -.5f;
        [SerializeField] private Renderer[] m_orbRenderers;
        [SerializeField] private bool m_movementEnabled;

        [Header("Orb Audio")]
        [SerializeField] private AudioSource m_audioSource;
        [SerializeField, Range(0f, 1f)] private float m_maxVolume = 1f;
        [SerializeField, Range(0f, 3f)] private float m_audioRampTime = 2f;
        private float m_audioVolume = 0f;

        [Header("Transform UVs to simulate rotation")]
        [SerializeField] private bool m_useRotationUVOffset;
        [SerializeField] private float m_offsetFactor;

        [Header("Camera Facing Rotation")]
        [SerializeField] private bool m_cameraFacing;
        [SerializeField] private Transform m_cameraFacingTransform;
        [SerializeField] private float m_rotationSpeed = 1f;

        [Header("SpringJoints")]
        [SerializeField] private GameObject[] m_springJoints;

        private Camera m_camera;
        private Vector2 m_currentUVOffset;
        private Quaternion m_lastCameraRotation;
        private bool m_orbTouched = false;
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

            var orbToHand = -(transform.position - m_leftHandTransform.position);
            var f = 1 / orbToHand.sqrMagnitude;
            orbToHand.Normalize();
            f *= Time.fixedDeltaTime;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, -orbToHand * (f / m_orbMass));
            Gizmos.DrawCube(transform.position - orbToHand * (f / m_orbMass), Vector3.one * .1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, orbToHand * (f * m_forceScaling));
            Gizmos.DrawCube(transform.position + orbToHand * (f * m_forceScaling), Vector3.one * .1f);
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

        public void SetDirCutoff(float dirCutoff)
        {
            m_handDirCutoff = Mathf.Clamp(dirCutoff, -1f, 1f);
        }
        private void Start()
        {
            m_orbProperties = new();
            m_originalPos = transform.position;

            m_camera = Camera.main;
            UnparentSpringJoints();
            StoreJointPositions();
        }

        private void SetExternalForceMultiplier(bool handAttracting, ParticleSystem ps)
        {
            var externalForces = ps.externalForces;
            if (handAttracting)
            {
                externalForces.multiplier += m_maxForcefieldMultiplier * (Time.deltaTime / m_forcefieldMultiplierRampUpTime);
                externalForces.multiplier = Mathf.Clamp(externalForces.multiplier, 0f, m_maxForcefieldMultiplier);
            }
            else
            {
                externalForces.multiplier = m_minForcefieldMultiplier;
            }
        }

        private void Update()
        {
            UpdateMaterialParameters();
            if (m_cameraFacing) CameraLookAt(m_camera, m_cameraFacingTransform, m_rotationSpeed);
            if (m_useRotationUVOffset) UpdateMaterialUVOffset();


            var targetPosition = Vector3.zero;
            var leftHandAttracting = false;
            var rightHandAttracting = false;
            var hands = 0;
            var leftHandToOrb = (transform.position - m_leftHandTransform.position).normalized;
            var leftDot = m_orbTouched ? 1 : Vector3.Dot(-m_leftHandTransform.up, leftHandToOrb);
            if ((m_orbTouched || !m_leftHandGrabbed.Active) && m_movementEnabled && leftDot > m_handDirCutoff)
            {
                targetPosition += Vector3.Lerp(m_originalPos, m_leftHandTransform.position - m_leftHandTransform.up * m_orbOffsetDistance, leftDot.ClampedMap(0, .75f, 0, 1));
                leftHandAttracting = true;
                hands++;
            }
            var rightHandToOrb = (transform.position - m_rightHandTransform.position).normalized;
            var rightDot = m_orbTouched ? 1 : Vector3.Dot(-m_rightHandTransform.up, rightHandToOrb);
            if ((m_orbTouched || !m_rightHandGrabbed.Active) && m_movementEnabled && rightDot > m_handDirCutoff)
            {
                targetPosition += Vector3.Lerp(m_originalPos, m_rightHandTransform.position - m_rightHandTransform.up * m_orbOffsetDistance, rightDot.ClampedMap(0, .75f, 0, 1));
                rightHandAttracting = true;
                hands++;
            }

            if (hands > 0)
            {
                targetPosition /= hands;
                m_audioVolume += m_maxVolume * Time.deltaTime / m_audioRampTime;
                m_audioVolume = Mathf.Clamp(m_audioVolume, 0, m_maxVolume);
            }
            else
            {
                targetPosition = m_originalPos;
                m_audioVolume -= m_maxVolume * Time.deltaTime / m_audioRampTime;
                m_audioVolume = Mathf.Clamp(m_audioVolume, 0, m_maxVolume);
            }

            var toTarget = targetPosition - transform.position;
            var force = toTarget.normalized * m_handForce.Evaluate(toTarget.magnitude);

            m_velocity += force * Time.deltaTime;

            //Set Audio Volume
            if (m_audioSource is not null)
            {
                m_audioSource.volume = m_audioVolume;
            }

            //Apply velocity
            transform.position += m_velocity * Time.deltaTime;

            //Apply Drag
            var multiplier = 1.0f - m_drag * Time.deltaTime;
            if (multiplier < 0.0f) multiplier = 0.0f;
            m_velocity *= multiplier;

            UpdateSprintJointPositions();

            AssignMaterialProperties();

            if (m_leftHandParticleSystem is not null)
            {
                SetExternalForceMultiplier(leftHandAttracting, m_leftHandParticleSystem);
            }

            if (m_rightHandParticleSystem is not null)
            {
                SetExternalForceMultiplier(rightHandAttracting, m_rightHandParticleSystem);
            }
        }

        private void FixedUpdate()
        {
            ApplyHandReactionForce(m_leftHandTransform, m_leftHandRigidbody);
            ApplyHandReactionForce(m_rightHandTransform, m_rightHandRigidbody);
        }

        private void ApplyHandReactionForce(Transform handTransform, Rigidbody handBody)
        {
            var orbToHand = -(transform.position - handTransform.position);
            var f = 1 / orbToHand.sqrMagnitude;
            orbToHand.Normalize();
            f *= Time.fixedDeltaTime;
            m_velocity += -orbToHand * (f / m_orbMass);
            handBody.AddForce(orbToHand * (f * m_forceScaling));
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
                m_orbTouched = true;
            }
        }
    }
}