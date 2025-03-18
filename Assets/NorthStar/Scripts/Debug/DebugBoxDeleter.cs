// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class DebugBoxDeleter : MonoBehaviour
    {
        public float MinDist;
        public Transform Platform;
        public Vector3 RelativePos;

        private void Awake()
        {
            RelativePos = transform.position - Platform.position;
        }

        private void LateUpdate()
        {
            if (Vector3.Distance(RelativePos, transform.position - Platform.position) > MinDist)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
