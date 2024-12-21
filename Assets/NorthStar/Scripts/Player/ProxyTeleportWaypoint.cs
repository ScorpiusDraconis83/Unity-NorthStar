// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Acts as a proxy for another teleporter waypoint so that the trigger location can be separate to the final destination
    /// </summary>
    public class ProxyTeleportWaypoint : TeleportWaypoint
    {
        [SerializeField] private TeleportWaypoint m_target;
        public override TeleportWaypoint DoWarp(Vector3 offset, Transform target, Transform head)
        {
            OnWarp.Invoke();
            return m_target.DoWarp(offset, target, head);
        }
        protected override void DrawGizmos()
        {
            if (m_target is not null)
            {
                base.DrawGizmos();
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(LosCheckTarget.position, m_target.LosCheckTarget.position);
            }
        }

    }
}
