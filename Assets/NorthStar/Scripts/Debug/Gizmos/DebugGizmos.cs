// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Draws a debug sphere
    /// </summary>
    public class DebugGizmos : MonoBehaviour
    {
        [SerializeField] private Color m_gizmoColor = Color.red;
        [SerializeField] private float m_radius = 0.5f;

        private void OnDrawGizmos()
        {
            Gizmos.color = m_gizmoColor;
            Gizmos.DrawWireSphere(transform.position, m_radius);
        }
    }
}
