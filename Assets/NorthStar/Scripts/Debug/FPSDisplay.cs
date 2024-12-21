// Copyright (c) Meta Platforms, Inc. and affiliates.
using TMPro;
using UnityEngine;

namespace NorthStar.DebugUtilities
{
    [RequireComponent(typeof(TMP_Text))]
    public class FPSDisplay : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text m_Text;

        private int m_fpsAccumulator;
        private float m_fpsNextPeriod;
        private int m_currentFps;

        private const float FPS_MEASURE_PERIOD = 0.5f;
        private const string DISPLAY = "{0} FPS";

        private void Start()
        {
            m_fpsNextPeriod = Time.realtimeSinceStartup + FPS_MEASURE_PERIOD;
        }

        private void Update()
        {
            // measure average frames per second
            m_fpsAccumulator++;
            if (Time.realtimeSinceStartup > m_fpsNextPeriod)
            {
                m_currentFps = (int)(m_fpsAccumulator / FPS_MEASURE_PERIOD);
                m_fpsAccumulator = 0;
                m_fpsNextPeriod += FPS_MEASURE_PERIOD;
                m_Text.text = string.Format(DISPLAY, m_currentFps);
            }
        }
    }
}