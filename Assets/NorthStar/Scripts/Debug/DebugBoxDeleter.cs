// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class DebugBoxDeleter : MonoBehaviour
    {
        public float minDist;
        public Transform platform;
        public Vector3 relativePos;

        private void Awake()
        {
            relativePos = transform.position - platform.position;
        }

        private void LateUpdate()
        {
            if (Vector3.Distance(relativePos, transform.position - platform.position) > minDist)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
