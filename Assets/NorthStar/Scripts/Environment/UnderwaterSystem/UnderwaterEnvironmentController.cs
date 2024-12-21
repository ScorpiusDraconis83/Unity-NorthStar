// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Handles updating some underwater-specific shader variables
    /// </summary>
    [ExecuteAlways]
    public class UnderwaterEnvironmentController : MonoBehaviour
    {
        [SerializeField] private UnderwaterEnvironmentData m_parameters;

        public UnderwaterEnvironmentData Parameters
        {
            get => m_parameters;
            set
            {
                m_parameters = value;
                UpdateCausticParameters();
            }
        }

        private int m_causticScaleProperty = Shader.PropertyToID("_CAUSTICSCALE");
        private int m_causticSpeedProperty = Shader.PropertyToID("_CAUSTICSPEED");
        private int m_causticTimeModulationProperty = Shader.PropertyToID("_CAUSTICTIME");
        private int m_causticEmissiveIntensityProperty = Shader.PropertyToID("_CAUSTICEMISSIVE");

        private int m_causticDistortionSpeedProperty = Shader.PropertyToID("_DistortionSpeed");
        private int m_causticDistortionIntensityProperty = Shader.PropertyToID("_DistortionIntensity");
        private int m_causticDistortionScaleProperty = Shader.PropertyToID("_DistortionScale");

        public void UpdateCausticParameters()
        {
            if (m_parameters == null) return;

            // Update shader keyword based on fog setting
            if (m_parameters.useUnderwaterFog)
            {
                Shader.EnableKeyword("_USE_UNDERWATER_FOG");
            }
            else
            {
                Shader.DisableKeyword("_USE_UNDERWATER_FOG");
            }

            // Update other parameters
            Shader.SetGlobalFloat(m_causticScaleProperty, m_parameters.causticScale);
            Shader.SetGlobalFloat(m_causticSpeedProperty, m_parameters.causticSpeed);
            Shader.SetGlobalFloat(m_causticTimeModulationProperty, m_parameters.causticTimeModulation);
            Shader.SetGlobalFloat(m_causticEmissiveIntensityProperty, m_parameters.causticEmissiveIntensity);

            Shader.SetGlobalFloat(m_causticDistortionIntensityProperty, m_parameters.distortionIntensity);
            Shader.SetGlobalVector(m_causticDistortionScaleProperty, m_parameters.distortionScale);
            Shader.SetGlobalVector(m_causticDistortionSpeedProperty, m_parameters.distortionSpeed);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateCausticParameters();
        }
#endif

        private void Awake()
        {
            UpdateCausticParameters();
        }

        private void OnEnable()
        {
            UpdateCausticParameters();
        }

        private void OnDisable()
        {
            // Ensure we disable the keyword when the controller is disabled
            Shader.DisableKeyword("_USE_UNDERWATER_FOG");
        }
    }
}