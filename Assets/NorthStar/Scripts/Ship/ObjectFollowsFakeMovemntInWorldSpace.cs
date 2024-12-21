// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Moves an object alongside any fake movement object without it being a child
    /// </summary>
    public class ObjectFollowsFakeMovemntInWorldSpace : MonoBehaviour
    {
        [SerializeField] private FakeMovement m_movementSource;
        private Vector3 m_position;
        private Quaternion m_rotation;
        private void Start()
        {
            m_position = transform.position;
            m_rotation = transform.rotation;
        }
        private void Update()
        {
            transform.position = m_movementSource.ConvertPoint(m_position);
            transform.rotation = m_movementSource.ConvertRotation(m_rotation);
        }
    }
}
