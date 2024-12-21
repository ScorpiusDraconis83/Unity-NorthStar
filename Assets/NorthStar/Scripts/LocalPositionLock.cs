// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Prevents objects from moving along an axis
    /// </summary>
    public class LocalPositionLock : MonoBehaviour
    {
        [SerializeField] private bool m_x, m_y, m_z;
        private Vector3 m_localPosition;
        private void OnEnable()
        {
            m_localPosition = transform.localPosition;
        }
        private void LateUpdate()
        {
            var pos = transform.localPosition;
            if (m_x)
                pos.x = m_localPosition.x;
            if (m_y)
                pos.y = m_localPosition.y;
            if (m_z)
                pos.z = m_localPosition.z;
            transform.localPosition = pos;
        }
    }
}
