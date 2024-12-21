// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Simple class that exposes ambient lighting intensity to event-based scripts
    /// </summary>
    public class SkyboxIntensityChange : MonoBehaviour
    {
        private float m_originalIntensity;
        private float m_targetIntensity;
        private float m_changeDuration;
        private float m_changeStartTime;

        public void SetSkyboxIntensityChangeTime(float duration)
        {
            m_changeDuration = duration;
        }

        public void ChangeSkyboxIntensity(float newIntensity)
        {
            m_originalIntensity = RenderSettings.ambientIntensity;
            m_targetIntensity = newIntensity;
            m_changeStartTime = Time.time;
            _ = StartCoroutine(nameof(LerpSkyboxIntensity));
        }

        private IEnumerator LerpSkyboxIntensity()
        {
            while (Time.time < m_changeStartTime + m_changeDuration)
            {
                var newIntensity = Mathf.Lerp(m_originalIntensity, m_targetIntensity, (Time.time - m_changeStartTime) / m_changeDuration);
                RenderSettings.ambientIntensity = newIntensity;
                yield return new WaitForEndOfFrame();
            }
            RenderSettings.ambientIntensity = m_targetIntensity;
            yield return null;
        }
    }
}
