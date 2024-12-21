// Copyright (c) Meta Platforms, Inc. and affiliates.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NorthStar
{
    /// <summary>
    /// Manages the current quality settings automatically based on the current active device
    /// 
    /// These settings are configurable and include things like target framerate, ASW and quality preset
    /// </summary>
    public class QualityControls : MonoBehaviour
    {
        public enum TargetFramerate
        {
            FPS72 = 72,
            FPS90 = 90,
            FPS120 = 120
        }

        [Serializable]
        public class QualityPreset
        {
            public List<OVRPlugin.SystemHeadset> Headsets = new();
            public int QualityIndex;
            public UniversalRenderPipelineAsset PipelineAsset;
            public bool UseASW;
            public bool UseDynamicFoveatedRendering;
            public OVRPlugin.FoveatedRenderingLevel FoveatedRenderingLevel;
            public TargetFramerate TargetFramerate;
        }

        [field: SerializeField] public List<QualityPreset> Presets { get; private set; }

        private QualityPreset m_currentPreset;

        /// <summary>
        /// Current space warp time remaining (in seconds)
        /// </summary>
        private float m_aswTimer;

        private void Start()
        {
            SwitchQualityLevel();
        }

        private void Update()
        {
            if (m_currentPreset == null)
                return;

            if (m_currentPreset.UseASW)
            {
                return;
            }
            else
            {
                m_aswTimer = Mathf.Max(m_aswTimer - Time.deltaTime, 0);

                if (m_aswTimer > 0 && !OVRManager.GetSpaceWarp())
                {
                    OVRManager.SetSpaceWarp(true);
                }
                else if (m_aswTimer == 0 && OVRManager.GetSpaceWarp())
                {
                    OVRManager.SetSpaceWarp(false);
                }
            }
        }

        private void SwitchQualityLevel()
        {
            var headsetType = OVRPlugin.GetSystemHeadsetType();
            foreach (var preset in Presets)
            {
                foreach (var targetHeadset in preset.Headsets)
                {
                    if (targetHeadset == headsetType)
                    {
                        SetQualityPreset(preset);
                        return;
                    }
                }
            }
        }

        private void SetQualityPreset(QualityPreset preset)
        {
            m_currentPreset = preset;
            QualitySettings.SetQualityLevel(preset.QualityIndex);
            OVRPlugin.systemDisplayFrequency = (float)preset.TargetFramerate;
            OVRManager.SetSpaceWarp(preset.UseASW);
            OVRPlugin.useDynamicFoveatedRendering = preset.UseDynamicFoveatedRendering;
            OVRPlugin.foveatedRenderingLevel = preset.FoveatedRenderingLevel;
        }

        /// <summary>
        /// Enable Application Space Warp for a given duration (in seconds). Single parameter method for use with Unity events
        /// </summary>
        /// <param name="delay"></param>
        public void EnableSpaceWarpForDuration(float duration)
        {
            m_aswTimer = Mathf.Max(m_aswTimer, duration);
        }

        /// <summary>
        /// Enable ASW until told to stop
        /// </summary>
        public void EnableSpaceWarp()
        {
            EnableSpaceWarpForDuration(float.PositiveInfinity);
        }

        /// <summary>
        /// Disable ASW immediately
        /// </summary>
        public void CancelSpaceWarp()
        {
            m_aswTimer = 0;
        }
    }
}
