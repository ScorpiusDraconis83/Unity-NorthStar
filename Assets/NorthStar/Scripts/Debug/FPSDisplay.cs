// Copyright (c) Meta Platforms, Inc. and affiliates.
using TMPro;
using UnityEngine;

namespace NorthStar.DebugUtilities
{
    [RequireComponent(typeof(TMP_Text))]
    public class FPSDisplay : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text m_text;

        private int m_fpsAccumulator;
        private float m_fpsNextPeriod;
        private int m_currentFps;
        private int m_lastDisplayedFps = -1;

        private const float FPS_MEASURE_PERIOD = 0.5f;
        private const int FPS_UPDATE_MULTIPLIER = (int)(1.0 / FPS_MEASURE_PERIOD);

        private void Start()
        {
            m_fpsNextPeriod = Time.realtimeSinceStartup + FPS_MEASURE_PERIOD;
        }

        private void Update()
        {
            // measure average frames per second
            m_fpsAccumulator++;
            var currentTime = Time.realtimeSinceStartup;
            if (currentTime > m_fpsNextPeriod)
            {
                m_currentFps = m_fpsAccumulator * FPS_UPDATE_MULTIPLIER;
                m_fpsAccumulator = 0;
                m_fpsNextPeriod = currentTime + FPS_MEASURE_PERIOD;

                if (m_currentFps != m_lastDisplayedFps)
                {
                    m_lastDisplayedFps = m_currentFps;
                    m_text.text = m_currentFps + " FPS";
                }
            }
        }
    }
}