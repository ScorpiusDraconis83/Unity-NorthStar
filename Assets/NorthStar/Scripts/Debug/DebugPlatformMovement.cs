// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using UnityEngine;

namespace NorthStar
{
    public class DebugPlatformMovement : MonoBehaviour
    {
        public float Speed;
        public float Rotation;

        [SerializeField, AutoSet] private Rigidbody m_rigidbody;

        private void FixedUpdate()
        {
            m_rigidbody.position += transform.forward * (Speed * Time.fixedDeltaTime);
            m_rigidbody.rotation *= Quaternion.Euler(0, Rotation * Time.fixedDeltaTime, 0);
        }
    }
}
