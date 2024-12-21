// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Playables;

namespace NorthStar
{
    /// <summary>
    /// Progresses a playable director based on the value from an interactable
    /// </summary>
    public class DivingPulley : MonoBehaviour
    {
        [SerializeField] private PlayableDirector m_pulleyDirector;
        [SerializeField] private PlayableDirector m_divingbellDirector;
        [SerializeField] private BaseJointInteractable<float> m_jointInteractable;

        private bool m_playedSecondDirector = false;

        private void Update()
        {
            m_pulleyDirector.time = m_jointInteractable.Value * m_pulleyDirector.duration;
            m_pulleyDirector.Evaluate();

            if (m_jointInteractable.Value >= 1 && !m_playedSecondDirector)
            {
                m_playedSecondDirector = true;
                m_divingbellDirector.Play();
            }
        }
    }
}
