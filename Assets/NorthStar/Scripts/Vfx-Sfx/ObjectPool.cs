// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// A generic object pool that manages object reuse in a convenient manner
    /// Instantiating a new object at runtime is both expensive and can cause GC hitches
    /// Use pools to avoid this, especially for frequently spawned things like VFX and sounds
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField, Tooltip("The initial size of the pool")] private int m_poolSize = 10;
        [SerializeField, Tooltip("The prefab template to instantiate (each pool only stoers one type of thing)")] private GameObject m_prefab;
        [SerializeField, Tooltip("The container where pooled objects will be stored when not in use")] private Transform m_container;
        [SerializeField, Tooltip("Whether to pre-warm the pool by creating all objects on awake")] private bool m_initializeOnAwake = false;

        private Queue<PooledObject> m_spawnQueue = new();
        private List<PooledObject> m_activeList = new();

        private void Awake()
        {
            if (m_initializeOnAwake)
                Initialize(m_prefab, m_poolSize);
        }

        public void Initialize(GameObject prefab, int poolSize)
        {
            m_activeList.Capacity = poolSize;
            m_poolSize = poolSize;
            m_prefab = prefab;
            if (m_container == null)
                m_container = transform;
            FillPool();
        }

        public void FillPool()
        {
            for (var i = 0; i < m_poolSize; i++)
            {
                var spawnedGo = Instantiate(m_prefab, m_container);
                if (!spawnedGo.TryGetComponent(out PooledObject pooledObject))
                {
                    pooledObject = spawnedGo.AddComponent<PooledObject>();
                }
                pooledObject.Owner = this;
                m_spawnQueue.Enqueue(pooledObject);
                spawnedGo.SetActive(false);
            }
        }

        public void Despawn(PooledObject pooledObject)
        {
            if (m_activeList.Remove(pooledObject))
            {
                m_spawnQueue.Enqueue(pooledObject);
                pooledObject.transform.parent = m_container;
                pooledObject.gameObject.SetActive(false);
                pooledObject.OnDespawn?.Invoke();
            }
        }

        public GameObject Spawn()
        {
            if (m_spawnQueue.Count == 0)
            {
                Despawn(m_activeList[0]);
            }
            var obj = m_spawnQueue.Dequeue();
            m_activeList.Add(obj);
            obj.gameObject.SetActive(true);
            obj.OnSpawn?.Invoke();
            return obj.gameObject;
        }
    }
}
