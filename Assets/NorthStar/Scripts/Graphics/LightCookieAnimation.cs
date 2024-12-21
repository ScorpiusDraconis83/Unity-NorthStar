// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NorthStar
{
    [RequireComponent(typeof(Light))]
    public class LightCookieAnimation : MonoBehaviour
    {
        public Texture2D[] CookieTextures;
        [Range(0.01f, 1.0f)]
        public float transitionDelay = 0.2f;
        public Vector2 scrollSpeed = new();

        private float m_lastTransitionTime;
        private int m_nextIndex;
        private Light m_thisLight;
        private UniversalAdditionalLightData m_lightData;

        private void Start()
        {
            m_thisLight = GetComponent<Light>();
            m_lightData = GetComponent<UniversalAdditionalLightData>();
        }

        private void Update()
        {
            m_lightData.lightCookieOffset += scrollSpeed * Time.deltaTime;
            if (CookieTextures.Length == 0)
            {
                return;
            }
            if (Time.time >= m_lastTransitionTime + transitionDelay)
            {
                m_thisLight.cookie = CookieTextures[m_nextIndex];
                m_nextIndex++;
                if (m_nextIndex >= CookieTextures.Length)
                {
                    m_nextIndex = 0;
                }
                m_lastTransitionTime = Time.time;
            }
        }
    }
}
