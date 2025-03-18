// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections;
using UnityEngine;

namespace NorthStar
{
    public class BoxReset : MonoBehaviour
    {
        private Vector3 m_originalPos;
        private MeshRenderer m_renderer;
        private Material m_material;
        private Collider m_collider;

        public float DissolveTime = .5f;

        private void Awake()
        {
            m_originalPos = transform.position;
            m_renderer = GetComponentInChildren<MeshRenderer>();
            m_collider = GetComponentInChildren<Collider>();
            m_material = m_renderer.material;
        }

        public void ResetPos()
        {
            Destroy(GetComponent<FixedJoint>());
            m_collider.enabled = false;
            _ = StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            var time = 0f;
            while (time < DissolveTime)
            {
                m_material.SetFloat("_Dissolve", 1 - time / DissolveTime);
                time += Time.deltaTime;
                yield return null;
            }

            m_material.SetFloat("_Dissolve", 1);
            transform.position = m_originalPos;
            m_collider.enabled = true;
        }
    }
}