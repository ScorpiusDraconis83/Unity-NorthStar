// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using UnityEngine;

namespace NorthStar
{
    public class HandDistanceLine : MonoBehaviour
    {
        private static readonly int s_alpha = Shader.PropertyToID("_Alpha");

        [SerializeField] private Transform m_anchor;
        [SerializeField] private AnimationCurve m_curve;

        [SerializeField, AutoSet] private LineRenderer m_lineRenderer;
        private Material m_material;

        private void Awake()
        {
            m_material = m_lineRenderer.material;
        }

        private void Update()
        {
            var distance = Vector3.Distance(m_anchor.position, transform.position);
            m_material.SetFloat(s_alpha, m_curve.Evaluate(distance));
            m_lineRenderer.SetPosition(1, transform.InverseTransformPoint(m_anchor.transform.position));
        }
    }
}
