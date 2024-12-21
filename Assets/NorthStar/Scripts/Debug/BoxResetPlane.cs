// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class BoxResetPlane : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {

            other.attachedRigidbody?.GetComponent<BoxReset>()?.ResetPos();
        }
    }
}
