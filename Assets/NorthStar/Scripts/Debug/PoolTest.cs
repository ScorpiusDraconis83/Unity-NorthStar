// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class PoolTest : MonoBehaviour
    {
        public EffectAsset EffectAsset;
        private void Update()
        {
            EffectAsset.Play(transform.position, transform.rotation);
        }
    }
}
