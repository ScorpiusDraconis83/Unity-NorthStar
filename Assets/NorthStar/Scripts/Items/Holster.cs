// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// An attach point for <see cref="Holsterable"/> objects
    /// </summary>
    public class Holster : MonoBehaviour
    {
        [SerializeField] private ConfigurableJoint m_joint;
        [SerializeField] private Holsterable m_item;
        [SerializeField] protected float m_range;

        private bool Isholstered
        {
            set
            {
                m_holstered = value;
                m_item.IsHolstered = value;
            }
            get => m_holstered;
        }

        private bool m_holstered = true;
        private bool m_forceReturn = false;

        [SerializeField] private int m_freezeFrameCount = 10;
        private int m_freezeCounter = 0;

        protected void Start()
        {
            m_item.OnGrab.AddListener(OnGrabbed);
            LockJoint();
        }

        private void Update()
        {
            if (m_freezeCounter > 0)
            {
                m_freezeCounter--;

                if (m_freezeCounter == 0)
                {
                    m_item.Unfreeze();
                }
            }

            var outOfRange = Vector3.Distance(transform.position, m_joint.connectedBody.position) >= m_range;

            if ((Isholstered || m_item.IsGrabbed) && !outOfRange)
            {
                return;
            }

            if (Vector3.Distance(transform.position, m_joint.connectedBody.position) <= 0.1f)
            {
                m_forceReturn = false;
                Isholstered = true;
            }
            else if (m_joint.connectedBody.position.y < transform.position.y - 0.15f || outOfRange)
            {
                m_forceReturn = true;

                m_item.Freeze();
                m_freezeCounter = m_freezeFrameCount;
            }

            if (m_forceReturn || Isholstered)
            {
                LockJoint();
            }
            else
            {
                ReleaseJoint();
            }
        }

        /// <summary>
        /// Lock the joint holding the <see cref="Holsterable"/> object, leaving the x axis unlocked
        /// </summary>
        private void LockJoint()
        {
            m_joint.xMotion = ConfigurableJointMotion.Locked;
            m_joint.yMotion = ConfigurableJointMotion.Locked;
            m_joint.zMotion = ConfigurableJointMotion.Locked;

            m_joint.angularXMotion = ConfigurableJointMotion.Locked;
            m_joint.angularYMotion = ConfigurableJointMotion.Locked;
            m_joint.angularZMotion = ConfigurableJointMotion.Locked;
        }

        /// <summary>
        /// Release the constraints on the joint holding the <see cref="Holsterable"/> object
        /// </summary>
        private void ReleaseJoint()
        {
            m_joint.xMotion = ConfigurableJointMotion.Free;
            m_joint.yMotion = ConfigurableJointMotion.Free;
            m_joint.zMotion = ConfigurableJointMotion.Free;

            m_joint.angularXMotion = ConfigurableJointMotion.Free;
            m_joint.angularYMotion = ConfigurableJointMotion.Free;
            m_joint.angularZMotion = ConfigurableJointMotion.Free;
        }

        /// <summary>
        /// Callback for grabbing and releases the <see cref="Holsterable"/>
        /// </summary>
        private void OnGrabbed()
        {
            m_freezeCounter = 0;
            m_item.Unfreeze();

            ReleaseJoint();
            m_forceReturn = false;
            Isholstered = false;
        }
    }
}
