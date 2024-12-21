// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Proxy for the rope system to detect and handle grabbing the rope using hands
    /// </summary>
    public class RopeGrabAnchor : MonoBehaviour
    {
        [SerializeField] private RopeSystem m_ropeSystem;
        [SerializeField, AutoSet] private PhysicsTransformer m_transform;
        [SerializeField, AutoSet] private Rigidbody m_body;
        [SerializeField] private Vector3 m_gripAxis;
        [SerializeField] private float m_gripWidth;
        private Quaternion m_handRotationOffset;

        private void Awake()
        {
            var interactable = GetComponentInChildren<HandGrabPose>();
            m_handRotationOffset = Quaternion.Inverse(transform.rotation) * interactable.transform.rotation;
        }

        public bool Grabbed { get; private set; } = false;

        public SyntheticHand Hand { get; set; }

        private RopeSystem.Anchor m_anchor;
        private bool m_invertedGrip;

        public Vector3 WorldBindAxis => Hand.transform.TransformDirection(m_gripAxis).normalized;


        private void OnEnable()
        {
            m_transform.OnInteraction += Grab;
            m_transform.OnEndInteraction += EndGrab;
        }

        private void OnDisable()
        {
            m_transform.OnInteraction -= Grab;
            m_transform.OnEndInteraction -= EndGrab;
        }

        private void Grab(HandGrabInteractor interactor)
        {
            if (Grabbed) return;
            m_anchor = m_ropeSystem.CreateAnchorViaRopeSim(transform.position, RopeSystem.AnchorType.Dynamic, gameObject);
            if (m_anchor is not null)
            {
                Grabbed = true;
                m_ropeSystem.OnRopeGrabbed();
                m_invertedGrip = false;

                if (m_ropeSystem.GetPrevAndNextAnchors(m_anchor, out var prevAnchor, out var nextAnchor))
                {
                    var ropeDir = m_ropeSystem.transform.TransformDirection((nextAnchor.Position - prevAnchor.Position).normalized);
                    m_invertedGrip = Vector3.Dot(ropeDir, WorldBindAxis) > 0.0f;
                }
            }
            m_body.isKinematic = false;
        }

        private void EndGrab(HandGrabInteractor interactor)
        {
            if (!Grabbed) return;
            Grabbed = false;
            m_ropeSystem.DestroyAnchor(m_anchor);
            m_ropeSystem.OnRopeReleased();
        }

        private void Update()
        {
            if (m_anchor == null)
            {
                Grabbed = false;
                m_anchor = null;
            }

            if (Grabbed)
            {
                m_anchor.BindAxis = m_ropeSystem.transform.InverseTransformDirection(m_invertedGrip ? -WorldBindAxis : WorldBindAxis);
                m_anchor.BindDistance = m_gripWidth / m_ropeSystem.TotalLength;
                m_body.isKinematic = false;
            }
            else
            {
                m_body.isKinematic = true;
                transform.position = m_ropeSystem.RopeSimulation.ClosestPointOnRope(Hand.transform.position);
                transform.rotation = Hand.transform.rotation * Quaternion.Inverse(m_handRotationOffset);
            }
        }
    }
}
