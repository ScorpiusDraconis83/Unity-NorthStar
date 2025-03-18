// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Controller for the sunray effect
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(ParticleSystem))]
    public class SunrayExtension : MonoBehaviour
    {
        [SerializeField] private Mesh m_wireframeMesh;
        [SerializeField] private float m_wireframeScale;
        [SerializeField] private GameObject m_rotationMarker;
        [SerializeField] private Mesh m_spawnMesh;
        [SerializeField, Range(0, 1)] private float m_directionalRotation = 0f;

        private ParticleSystem m_sunrayParticleSystem;
        private MeshFilter m_meshFilter;
        private MeshRenderer m_meshRenderer;

        private void InitilizeParticleSystem()
        {
            m_sunrayParticleSystem = GetComponent<ParticleSystem>();
            m_meshFilter = GetComponent<MeshFilter>();
            m_meshRenderer = GetComponent<MeshRenderer>();
            if (m_meshFilter.sharedMesh != m_spawnMesh)
            {
                m_meshFilter.sharedMesh = m_spawnMesh;
            }

            var psShape = m_sunrayParticleSystem.shape;
            psShape.enabled = true;
            psShape.shapeType = ParticleSystemShapeType.MeshRenderer;
            psShape.meshRenderer = m_meshRenderer;
        }

        private void UpdateRayDirection()
        {
            var worldDirection = m_rotationMarker.transform.rotation * Vector3.right;
            var pointPositions = m_meshFilter.sharedMesh.vertices;
            var normals = m_meshFilter.sharedMesh.normals;

            for (var i = 0; i < normals.Length; i++)
            {
                normals[i] = worldDirection.normalized;
                var direction = (transform.TransformPoint(pointPositions[i]) - transform.localPosition).normalized;
                normals[i] = Vector3.Slerp(normals[i], direction, m_directionalRotation);
            }
            m_meshFilter.sharedMesh.normals = normals;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireMesh(m_wireframeMesh, gameObject.transform.position, m_rotationMarker.transform.rotation,
             new Vector3(m_wireframeScale, m_wireframeScale, m_wireframeScale));

            var positions = m_spawnMesh.vertices;
            var normals = m_spawnMesh.normals;

            for (var i = 0; i < positions.Length; i++)
            {
                var pointPosition = transform.TransformPoint(positions[i]);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(pointPosition, 0.1f);
                Gizmos.DrawRay(pointPosition, normals[i] * 3);
            }
        }

        private void OnValidate()
        {
            InitilizeParticleSystem();
            UpdateRayDirection();
        }
#endif

        private void Awake()
        {
            InitilizeParticleSystem();
            UpdateRayDirection();
        }
    }
}