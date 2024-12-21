// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Prevents objects from rotating around an axis
    /// </summary>
    public class LocalAxisLock : MonoBehaviour
    {
        [SerializeField] private bool m_x, m_y, m_z;

        private void LateUpdate()
        {
            var rot = transform.localEulerAngles;
            if (m_x)
                rot.x = 0;
            if (m_y)
                rot.y = 0;
            if (m_z)
                rot.z = 0;
            transform.localEulerAngles = rot;
        }
    }
}
