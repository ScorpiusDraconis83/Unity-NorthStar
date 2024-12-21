// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Freezes an object at a specific position, used for barrel rolling activity
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class LocalPositionConstraints : MonoBehaviour
    {
        public bool freezeXPosition = false;
        public bool freezeYPosition = false;
        public bool freezeZPosition = false;

        private Vector3 m_initialLocalPosition;
        private Vector3 m_frozenPosition;

        private void Awake()
        {
            m_initialLocalPosition = transform.localPosition;
        }

        private void FixedUpdate()
        {
            m_frozenPosition = transform.localPosition;
            if (freezeXPosition)
            {
                m_frozenPosition.x = m_initialLocalPosition.x;
            }

            if (freezeYPosition)
            {
                m_frozenPosition.y = m_initialLocalPosition.y;
            }

            if (freezeZPosition)
            {
                m_frozenPosition.z = m_initialLocalPosition.z;
            }

            transform.localPosition = m_frozenPosition;
        }
    }
}