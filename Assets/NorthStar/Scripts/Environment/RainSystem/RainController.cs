// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Serialization;

namespace NorthStar
{
    /// <summary>
    /// Controls the rain effect and updates related shader variables
    /// </summary>
    [ExecuteAlways]
    public class RainController : MonoBehaviour
    {
        private const string RAIN_KEYWORD = "_USE_RAIN";

        [FormerlySerializedAs("m_settings")]
        [SerializeField]
        private RainData mData;

        [Header("Runtime Controls")]
        [SerializeField]
        private bool m_autoUpdateInEditor = true;

        [SerializeField] private bool m_enableRain = false;
        [SerializeField] private float m_updateInterval = 0.1f;

        private float m_lastUpdateTime;
        private bool m_wasRainEnabled;

        private void OnEnable()
        {
            if (mData != null)
            {
                mData.UpdateShaderProperties();
            }

            UpdateRainKeyword();
        }

        private void OnDisable()
        {
            // Ensure keyword is disabled when component is disabled
            if (m_enableRain)
            {
                Shader.DisableKeyword(RAIN_KEYWORD);
            }
        }

        private void Update()
        {
            // Check if rain enabled state changed
            if (m_wasRainEnabled != m_enableRain)
            {
                UpdateRainKeyword();
                m_wasRainEnabled = m_enableRain;
            }

            // Skip update if we're in editor and auto-update is disabled
            if (!Application.isPlaying && !m_autoUpdateInEditor)
                return;
        }

        private void UpdateRainKeyword()
        {
            if (m_enableRain)
            {
                Shader.EnableKeyword(RAIN_KEYWORD);
            }
            else
            {
                Shader.DisableKeyword(RAIN_KEYWORD);
            }

            //onRainEnabledChanged?.Invoke(m_enableRain);
        }

        private void UpdateRainSettings()
        {
            if (mData == null)
                return;

            mData.UpdateShaderProperties();
        }

        public void SetRainEnabled(bool enabled)
        {
            if (m_enableRain != enabled)
            {
                m_enableRain = enabled;
                UpdateRainKeyword();
            }
        }

        // Quick methods for common operations
        public void StartRain()
        {
            SetRainEnabled(true);
        }

        public void StopRain()
        {
            SetRainEnabled(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_autoUpdateInEditor)
            {
                UpdateRainSettings();
                if (m_wasRainEnabled != m_enableRain)
                {
                    UpdateRainKeyword();
                    m_wasRainEnabled = m_enableRain;
                }
            }
        }
#endif
    }
}