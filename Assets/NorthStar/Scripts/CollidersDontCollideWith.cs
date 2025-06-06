// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Prevents colliders on one object colliding with other select colliders
    /// </summary>
    public class CollidersDontCollideWith : MonoBehaviour
    {
        [SerializeField] private List<Collider> m_others;
        private void Start()
        {
            foreach (var collider in GetComponents<Collider>())
            {
                foreach (var collider2 in m_others)
                {
                    Physics.IgnoreCollision(collider, collider2);
                }
            }
        }
    }
}
