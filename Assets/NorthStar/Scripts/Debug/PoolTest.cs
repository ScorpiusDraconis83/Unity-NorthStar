// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class PoolTest : MonoBehaviour
    {
        public EffectAsset effectAsset;
        private void Update()
        {
            effectAsset.Play(transform.position, transform.rotation);
        }
    }
}
