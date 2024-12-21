// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using Meta.Utilities;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Efficiently manages spawning VFX using object pools
    /// </summary>
    public class VfxManager : Singleton<VfxManager>
    {
        private Dictionary<EffectAsset, ObjectPool> m_pools = new();

        public void SpawnLocal(EffectAsset effect, Vector3 position, Quaternion rotation, bool inBoatSpace = false, float intensity = 1)
        {
            if (!m_pools.ContainsKey(effect))
            {
                var newPoolGo = new GameObject(effect.name);
                newPoolGo.transform.parent = transform;
                var newPool = newPoolGo.AddComponent<ObjectPool>();
                newPool.Initialize(effect.VfxObjectPrefab, effect.PoolSize);
                m_pools[effect] = newPool;
            }
            var obj = m_pools[effect].Spawn();

            obj.transform.localPosition = position;
            obj.transform.localRotation = rotation;
            if (BoatController.Instance != null)
                if (inBoatSpace) obj.transform.parent = BoatController.Instance.transform;
            obj.GetComponent<EffectObject>().Play(intensity);
        }

        public static void Spawn(EffectAsset effect, Vector3 position, Quaternion rotation, bool inBoatSpace = false, float intensity = 1)
        {
            Instance.SpawnLocal(effect, position, rotation, inBoatSpace, intensity);
        }
    }
}
