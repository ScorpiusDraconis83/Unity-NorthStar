// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Used as part of the ObjectPool class to tag pooled objects
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        public delegate void Callback();
        public Callback OnSpawn, OnDespawn;
        public ObjectPool Owner;
        public void Despawn()
        {
            Owner.Despawn(this);
        }
    }
}
