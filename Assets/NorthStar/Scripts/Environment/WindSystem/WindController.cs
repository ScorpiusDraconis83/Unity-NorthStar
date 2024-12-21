// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Handles updating shader variables related to wind speed and direction
    /// </summary>
    [ExecuteInEditMode] // Add this to make it run in edit mode
    public class WindController : MonoBehaviour
    {
        [SerializeField] private WindData windData;
        private float m_lastUpdateTime;
        private const float UPDATE_INTERVAL = 0.016f; // ~60fps update rate

        // Cached shader property IDs
        private static readonly int s_randomOffsetId = Shader.PropertyToID("_RandomOffset");
        private static readonly int s_windDirectionId = Shader.PropertyToID("_Wind_Direction");
        private static readonly int s_windIntensityId = Shader.PropertyToID("_Wind_Intensity");
        private static readonly int s_leadAmountId = Shader.PropertyToID("_Lead_Amount");

        // Primary Wind
        private static readonly int s_primaryWindSpeedId = Shader.PropertyToID("_Primary_Wind_Speed");
        private static readonly int s_primaryFrequencyId = Shader.PropertyToID("_Primary_Frequency");
        private static readonly int s_primaryAmplitudeId = Shader.PropertyToID("_Primary_Amplitude");

        // Secondary Wind
        private static readonly int s_secondaryWindSpeedId = Shader.PropertyToID("_Secondary_Wind_Speed");
        private static readonly int s_secondaryFrequencyId = Shader.PropertyToID("_Secondary_Frequency");
        private static readonly int s_secondaryAmplitudeId = Shader.PropertyToID("_Secondary_Amplitude");

        // Verticle Leaf
        private static readonly int s_verticleLeafSpeedId = Shader.PropertyToID("_Verticle_Leaf_Speed");
        private static readonly int s_verticleLeafFrequencyId = Shader.PropertyToID("_Verticle_Leaf_Frequency");
        private static readonly int s_verticleLeafAmplitudeId = Shader.PropertyToID("_Verticle_Leaf_Amplitude");

        // Trunk
        private static readonly int s_trunkWindSpeedId = Shader.PropertyToID("_Trunk_Wind_Speed");
        private static readonly int s_trunkFrequencyId = Shader.PropertyToID("_Trunk_Frequency");
        private static readonly int s_trunkAmplitudeId = Shader.PropertyToID("_Trunk_Amplitude");
        private static readonly int s_trunkWindIntensityId = Shader.PropertyToID("_Trunk_Wind_Intensity");

        public WindData WindData
        {
            get => windData;
            set
            {
                windData = value;
                if (windData != null)
                {
                    UpdateShaderParameters();
                }
            }
        }

        private void OnEnable()
        {
            UpdateShaderParameters();
        }

        private void Update()
        {
            // Throttle updates in edit mode to avoid excessive updates
            if (!Application.isPlaying)
            {
                var currentTime = Time.realtimeSinceStartup;
                if (currentTime - m_lastUpdateTime < UPDATE_INTERVAL)
                    return;

                m_lastUpdateTime = currentTime;
            }

            UpdateShaderParameters();
        }

        private void UpdateShaderParameters()
        {
            if (windData == null) return;

            // Update all shader parameters globally using cached property IDs
            Shader.SetGlobalFloat(s_randomOffsetId, windData.RandomOffset);
            Shader.SetGlobalVector(s_windDirectionId, windData.WindDirection);
            Shader.SetGlobalFloat(s_windIntensityId, windData.WindIntensity);
            Shader.SetGlobalFloat(s_leadAmountId, windData.LeadAmount);

            Shader.SetGlobalFloat(s_primaryWindSpeedId, windData.PrimaryWindSpeed);
            Shader.SetGlobalFloat(s_primaryFrequencyId, windData.PrimaryFrequency);
            Shader.SetGlobalFloat(s_primaryAmplitudeId, windData.PrimaryAmplitude);

            Shader.SetGlobalFloat(s_secondaryWindSpeedId, windData.SecondaryWindSpeed);
            Shader.SetGlobalFloat(s_secondaryFrequencyId, windData.SecondaryFrequency);
            Shader.SetGlobalFloat(s_secondaryAmplitudeId, windData.SecondaryAmplitude);

            Shader.SetGlobalFloat(s_verticleLeafSpeedId, windData.VerticleLeafSpeed);
            Shader.SetGlobalFloat(s_verticleLeafFrequencyId, windData.VerticleLeafFrequency);
            Shader.SetGlobalFloat(s_verticleLeafAmplitudeId, windData.VerticleLeafAmplitude);

            Shader.SetGlobalFloat(s_trunkWindSpeedId, windData.TrunkWindSpeed);
            Shader.SetGlobalFloat(s_trunkFrequencyId, windData.TrunkFrequency);
            Shader.SetGlobalFloat(s_trunkAmplitudeId, windData.TrunkAmplitude);
            Shader.SetGlobalFloat(s_trunkWindIntensityId, windData.TrunkWindIntensity);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) // Check if object still exists
                {
                    UpdateShaderParameters();
                }
            };
        }
#endif
    }
}