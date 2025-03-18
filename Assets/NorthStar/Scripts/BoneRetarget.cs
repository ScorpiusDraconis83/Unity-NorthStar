// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace NorthStar
{
    public class BoneRetarget : MonoBehaviour
    {
        public Transform TargetTransform;

        private Quaternion m_initialOrientation;

        protected void Awake()
        {
            m_initialOrientation = transform.localRotation;
        }

        protected void LateUpdate()
        {
            var localRight = -transform.parent.InverseTransformDirection(transform.right);
            var localFwd = m_initialOrientation * Vector3.forward;
            var newOrientation = Quaternion.LookRotation(localRight, localFwd) *
                                 Quaternion.AngleAxis(90f, Vector3.up) * Quaternion.AngleAxis(-90f, Vector3.right);

            var twist = Quaternion.Inverse(newOrientation) * transform.localRotation;
            TargetTransform.localRotation *= twist;
            transform.localRotation = newOrientation;
        }

    }
}