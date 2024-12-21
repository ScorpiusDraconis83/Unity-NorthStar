// Copyright (c) Meta Platforms, Inc. and affiliates.
using System;
using Meta.Utilities;
using Oculus.Interaction;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Controller for boat logic and movement system
    /// </summary>
    public class BoatController : Singleton<BoatController>
    {
        public enum Mode
        {
            FakeMovement,
            PhysicsPlatform
        }

        [SerializeField, Section("Movement")] private Mode m_boatMovementMode = Mode.PhysicsPlatform;
        [SerializeField] private float m_maxSpeed = 1.0f;
        [SerializeField] private float m_pitchAngle = 15.0f;
        [SerializeField] private float m_rollAngle = 15.0f;
        [SerializeField] private float m_turnRollAngle = 15.0f;
        [SerializeField] private float m_rotationSpeed = 1.0f;
        [SerializeField] private float m_bobAmount = 2.0f;
        [SerializeField] private float m_bobSpeed = 2.0f;
        [SerializeField] private float m_turnSpeed = 15.0f;

        [SerializeField, Range(0, 1), Section("Environment")] private float m_reorientGravityStrength = 1;
        [field: SerializeField, Range(0, 2)] public float SecondaryMotionStrength { get; set; } = 1;

        [SerializeField, Section("References")] private WheelController m_angleController;
        [SerializeField] private SailAngleController m_sailController;
        [SerializeField] private Transform m_player;
        [SerializeField] private Collider m_boatBoundaryCollider;
        [SerializeField, AutoSet] private Rigidbody m_rigidbody;
        [SerializeField, AutoSet] private ParentedTransform m_parentedTransform;

        [field: SerializeField] public FakeMovement MovementSource { get; private set; }

        private Vector3 m_originalPosition = Vector3.zero;
        private Quaternion m_originalRotation = Quaternion.identity;
        private Vector3 m_noiseOffsets = Vector3.zero;
        private Vector3 m_initialGravity;

        private Vector3 m_position = Vector3.zero;

        private static readonly int s_giantWaveOffset = Shader.PropertyToID("_GiantWaveOffset");

        [Serializable]
        public struct ReactionMovementData
        {
            public AnimationCurve EaseCurve;
            public Vector3 PositionOffset;
            public Vector3 AngularOffset;
        }

        public struct TransformOffset
        {
            public Vector3 Position;
            public Quaternion Rotation;
        }

        public TransformOffset?[] ReactionMovement { get; private set; } = new TransformOffset?[10];

        public bool Stopped { get; private set; } = false;

        [field: SerializeField] public float HeadingAngle { get; set; }
        [SerializeField] private bool m_useHeadingLimits = false;
        [SerializeField] private float m_minHeading = -180, m_maxHeading = 180;

        [ContextMenu("Start boat")]
        public void StartBoat()
        {
            if (!Stopped) return;
            Stopped = false;
        }

        [ContextMenu("Stop boat")]
        public void StopBoat()
        {
            if (Stopped) return;
            Stopped = true;
            MovementSource.Sync();
        }

        protected override void InternalAwake()
        {
            m_originalPosition = transform.position;
            m_originalRotation = transform.rotation;
            m_position = m_originalPosition;
            m_initialGravity = Physics.gravity;
        }

        private void Update()
        {
            var comfortStrength = GlobalSettings.PlayerSettings.BoatMovementStrength;
            var comfortReactionStrength = GlobalSettings.PlayerSettings.BoatReactionStrength;

            var speed = Stopped ? 0 : m_sailController?.GetSailSpeed() ?? 0;
            var normalizedSpeed = speed / m_maxSpeed;
            var turn = Stopped ? 0 : m_angleController?.Value ?? 0;

            m_noiseOffsets.x += Time.deltaTime * m_rotationSpeed; // Noise sample offset for pitch angle
            m_noiseOffsets.y += Time.deltaTime * m_rotationSpeed; // Noise sample offset for roll angle
            m_noiseOffsets.z += Time.deltaTime * m_bobSpeed; // Noise sample offset for bobbing

            var pitch = Mathf.PerlinNoise(m_noiseOffsets.x, 0).Map(0, 1, -1, 1) * m_pitchAngle * (normalizedSpeed * 0.8f + 0.2f);
            var roll = Mathf.PerlinNoise(0, m_noiseOffsets.y).Map(0, 1, -1, 1) * m_rollAngle * (normalizedSpeed * 0.8f + 0.2f) - m_turnRollAngle * turn * normalizedSpeed;

            var horizontalRotation = Quaternion.AngleAxis(HeadingAngle, Vector3.up);
            var forward = horizontalRotation * Vector3.forward;
            var right = horizontalRotation * Vector3.right;

            var rotation = horizontalRotation * Quaternion.AngleAxis(roll * SecondaryMotionStrength * comfortStrength, forward) * Quaternion.AngleAxis(pitch * SecondaryMotionStrength * comfortStrength, right);

            HeadingAngle += m_turnSpeed * turn * normalizedSpeed * Time.deltaTime;
            if (m_useHeadingLimits)
            {
                HeadingAngle = Mathf.Clamp(HeadingAngle, m_minHeading, m_maxHeading);
            }

            m_position += forward * (speed * Time.deltaTime);
            m_position.y = m_originalPosition.y + Mathf.PerlinNoise1D(m_noiseOffsets.z) * (normalizedSpeed * 0.8f + 0.2f) * m_bobAmount * SecondaryMotionStrength * comfortStrength;

            MovementSource.CurrentPosition = m_position;
            MovementSource.CurrentRotation = rotation;

            foreach (var reaction in ReactionMovement)
            {
                if (reaction != null)
                {
                    GetReactionOffsets(reaction.Value, out var reactionPos, out var reactionRot);
                    MovementSource.CurrentPosition += reactionPos * comfortStrength;
                    MovementSource.CurrentRotation = Quaternion.Slerp(MovementSource.CurrentRotation, MovementSource.CurrentRotation * reactionRot, comfortReactionStrength);
                }
            }

            Shader.SetGlobalVector(s_giantWaveOffset, MovementSource.CurrentPosition);
        }

        private void FixedUpdate()
        {
            // Inverse boats rotation to fake gravity from the perspective of those in the boat
            Physics.gravity = Quaternion.Slerp(Quaternion.identity, Quaternion.Inverse(MovementSource.CurrentRotation), m_reorientGravityStrength) * m_initialGravity;
        }

        private void GetReactionOffsets(TransformOffset reaction, out Vector3 position, out Quaternion rotation)
        {
            var headingRotation = m_originalRotation * Quaternion.AngleAxis(HeadingAngle, Vector3.up);
            position = headingRotation * reaction.Position;
            rotation = headingRotation * reaction.Rotation;
        }

        public void ApplyReactionMovement(TransformOffset reaction)
        {
            GetReactionOffsets(reaction, out var position, out var rotation);
            transform.position = m_originalPosition + position;
            transform.rotation = m_originalRotation * rotation;
        }

        public void PreviewReactionMovementInEditor(TransformOffset reaction, int slot)
        {
            transform.position = m_originalPosition;
            transform.rotation = m_originalRotation;
            Shader.SetGlobalVector(s_giantWaveOffset, transform.position);

            ReactionMovement[slot] = reaction;

            foreach (var r in ReactionMovement)
            {
                if (r != null)
                {
                    GetReactionOffsets(r.Value, out var reactionPos, out var reactionRot);
                    transform.position += reactionPos;
                    transform.rotation *= reactionRot;
                }
            }
        }

        public void ResetToOriginalTransform()
        {
            transform.position = m_originalPosition;
            transform.rotation = m_originalRotation;
        }

        public static Vector3 WorldToBoatSpace(Vector3 point)
        {
            return Instance.MovementSource.CurrentRotation * (point - Instance.transform.position) + Instance.MovementSource.CurrentPosition;
        }

        public static Quaternion WorldToBoatSpace(Quaternion quaternion)
        {
            return quaternion * Quaternion.Inverse(Instance.transform.rotation) * Instance.MovementSource.CurrentRotation;
        }

        private void OnValidate()
        {
            if (m_angleController == null)
            {
                m_angleController = FindAnyObjectByType<WheelController>(findObjectsInactive: FindObjectsInactive.Include);
            }
        }
    }
}
