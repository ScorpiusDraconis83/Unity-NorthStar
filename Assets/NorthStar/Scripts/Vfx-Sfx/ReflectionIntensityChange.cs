// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Simple class that exposes reflection intensity to event-based scripts
    /// </summary>
    public class ReflectionIntensityChange : MonoBehaviour
    {
        private float m_originalIntensity;
        private float m_targetIntensity;
        private float m_changeDuration;
        private float m_changeStartTime;

        public ReflectionProbe[] ReflectionProbes;

        [Tooltip("If setting this, ensure you do not also alter the intensity of the probe used for world reflection intensity")]
        public bool UpdateGlobalReflectionIntensity = true;

        public void SetReflectionIntensityChangeTime(float duration)
        {
            m_changeDuration = duration;
        }
        public void ChangeReflectionIntensity(float newIntensity)
        {
            m_originalIntensity = RenderSettings.reflectionIntensity;
            m_targetIntensity = newIntensity;
            m_changeStartTime = Time.time;
            _ = StartCoroutine(nameof(LerpReflectionIntensity));
        }

        private IEnumerator LerpReflectionIntensity()
        {
            while (Time.time < m_changeStartTime + m_changeDuration)
            {
                var newIntensity = Mathf.Lerp(m_originalIntensity, m_targetIntensity, (Time.time - m_changeStartTime) / m_changeDuration);
                if (UpdateGlobalReflectionIntensity)
                {
                    RenderSettings.reflectionIntensity = newIntensity;
                }

                foreach (var probe in ReflectionProbes)
                {
                    probe.intensity = newIntensity;
                }
                yield return new WaitForEndOfFrame();
            }
            if (UpdateGlobalReflectionIntensity)
            {
                RenderSettings.reflectionIntensity = m_targetIntensity;
            }
            yield return null;
        }
    }
}
