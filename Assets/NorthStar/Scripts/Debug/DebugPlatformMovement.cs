// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using UnityEngine;

namespace NorthStar
{
    public class DebugPlatformMovement : MonoBehaviour
    {
        public float speed;
        public float rotation;

        [SerializeField, AutoSet] private Rigidbody rb;

        private void FixedUpdate()
        {
            rb.position += transform.forward * (speed * Time.fixedDeltaTime);
            rb.rotation *= Quaternion.Euler(0, rotation * Time.fixedDeltaTime, 0);
        }
    }
}
