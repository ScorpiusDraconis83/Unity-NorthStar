// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NorthStar
{
    /// <summary>
    /// This class exposes a default value "_Transition" inside material to the inspector so that it can be animated for easier implementation of effects by engineers
    /// </summary>
    [ExecuteInEditMode]
    public class PassTransitionVariableToMaterial : MonoBehaviour
    {
        public bool IsDecal = false;

        [Tooltip("Transition 1 is effect full on, transition 0 is effect off. (Defaulting to 1 so effect can be seen during setup)")]
        [Range(0.0f, 1.0f)]
        public float Transition = 1;

        private Material m_effectMaterial;
        private bool m_setupCompleted = false;

        private void Awake()
        {
            if (m_effectMaterial = null)
            {
                RunSetup();
            }
        }

        private void Update()
        {
            if (!m_setupCompleted)
            {
                RunSetup();
            }

            if (m_setupCompleted)
            {
                if (m_effectMaterial.HasFloat("_Transition"))
                {
                    m_effectMaterial.SetFloat("_Transition", Transition);
                }
            }
        }

        private void RunSetup()
        {
            if (IsDecal)
            {
                var decal = GetComponent<DecalProjector>();
                if (decal != null)
                {
                    m_effectMaterial = decal.material;
                    m_setupCompleted = true;
                }
            }
            else
            {
                m_effectMaterial = GetComponent<Renderer>().material;
                m_setupCompleted = true;
            }
        }
    }
}
