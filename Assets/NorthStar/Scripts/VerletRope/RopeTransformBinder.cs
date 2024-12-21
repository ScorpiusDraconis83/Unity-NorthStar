// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Binds part of a rope to this objects transform position (used for things like the harpoon)
    /// </summary>
    public class RopeTransformBinder : MonoBehaviour
    {
        [SerializeField] public int NodeIndex, BindIndex;
        [SerializeField] public BurstRope m_rope;
        public bool useBoatSpace;
        [SerializeField] private bool m_lateUpdate = false;

        private void OnEnable()
        {
            Enable();
        }
        private void OnDisable()
        {
            if (m_rope == null)
                return;
            var bind = m_rope.Binds[BindIndex];
            bind.Bound = false;
            m_rope.Binds[BindIndex] = bind;
        }

        public void Enable()
        {
            if (m_rope == null)
                return;
            var bind = m_rope.Binds[BindIndex];
            bind.Bound = true;
            m_rope.Binds[BindIndex] = bind;
        }

        private void Update()
        {
            if (m_lateUpdate) return;
            var bind = m_rope.Binds[BindIndex];
            bind.Target = m_rope.ToRopeSpace(useBoatSpace ? BoatController.WorldToBoatSpace(transform.position) : transform.position);
            bind.Index = NodeIndex;
            m_rope.Binds[BindIndex] = bind;
        }
        private void LateUpdate()
        {
            if (!m_lateUpdate) return;
            var bind = m_rope.Binds[BindIndex];
            bind.Target = m_rope.ToRopeSpace(useBoatSpace ? BoatController.WorldToBoatSpace(transform.position) : transform.position);
            bind.Index = NodeIndex;
            m_rope.Binds[BindIndex] = bind;
        }
    }
}
