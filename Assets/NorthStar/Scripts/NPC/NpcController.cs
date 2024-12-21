// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// High level script to manage the NPCRigController
    /// </summary>
    public class NpcController : MonoBehaviour
    {
        [SerializeField] private Transform m_player;
        [SerializeField] private bool m_noticePlayer = true;
        [SerializeField] private float m_triggerDistance = 5;
        [SerializeField] private float m_triggerAngle = 90;

        private NpcRigController m_rigController;

        [SerializeField, AutoSet(typeof(Animator))] private Animator m_animator;
        private Transform m_hips;

        private void Start()
        {
            m_rigController = GetComponent<NpcRigController>();
            m_hips = m_animator.GetBoneTransform(HumanBodyBones.Hips);
        }

        private void Update()
        {
            if (m_rigController != null)
            {
                m_rigController.LookAtTarget = m_noticePlayer && m_player != null && IsTargetInRange(m_player, m_triggerDistance) && GetAngleFromTarget(m_player) < m_triggerAngle
                    ? m_player
                    : null;
            }
        }


        /// <summary>
        /// Return if target is close enough and in the cone of vision
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsTargetInRange(Transform target, float triggerDistance)
        {
            return triggerDistance < 0 || Vector3.Distance(target.position, m_hips.position) < triggerDistance;
        }

        /// <summary>
        /// Get angle between the NPC forward and the targets position in degrees
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public float GetAngleFromTarget(Transform target)
        {
            var targetDirection = target.position - m_hips.position;
            var angle = Vector3.Angle(targetDirection, m_hips.up);
            return angle;
        }
    }
}
