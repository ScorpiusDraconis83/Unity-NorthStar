// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Sets the sail values from a unity event
    /// </summary>
    public class NarrativeSailSetter : MonoBehaviour
    {
        [SerializeField] private float m_transitionTime;
        [SerializeField] private float m_startValue, m_targetValue;
        [SerializeField] private SailAngleController[] m_sailAngleControllers;

        private float m_timer;
        private bool m_run;
        public void Trigger()
        {
            m_timer = 0;
            m_run = true;
        }

        private void Update()
        {
            if (m_run)
            {
                m_timer += Time.deltaTime;
                var val = Mathf.Lerp(m_startValue, m_targetValue, Easeing(Mathf.Clamp01(m_timer / m_transitionTime)));
                foreach (var controller in m_sailAngleControllers)
                {
                    controller.SetSailHeight(val);
                }
                if (val == 1)
                    m_run = false;
            }
        }

        private float Easeing(float t)
        {
            return 1 - Mathf.Cos(t * Mathf.PI / 2);
        }
    }
}
