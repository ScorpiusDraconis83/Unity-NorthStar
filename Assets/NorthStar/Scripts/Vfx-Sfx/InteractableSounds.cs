// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Controls the volume of interactable sounds based user interaction speed
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class InteractableSounds : MonoBehaviour
    {
        [SerializeField, AutoSet] private BaseJointInteractable<float> m_jointInteractable;
        [SerializeField, AutoSet] private AudioSource m_audio;
        [SerializeField] private AnimationCurve m_animationCurve;
        private float m_lastValue;
        private bool m_ready;

        private void Awake()
        {
            m_audio.volume = 0;
        }
        private void Start()
        {
            m_audio.Play();
        }
        private void LateUpdate()
        {
            if (!m_ready)
            {
                m_lastValue = m_jointInteractable.Value;
                m_ready = true;
                return;
            }
            var speedEstimate = Mathf.Abs(m_lastValue - m_jointInteractable.Value) / Time.deltaTime;

            m_audio.volume = m_animationCurve.Evaluate(speedEstimate);

            m_lastValue = m_jointInteractable.Value;
        }
    }
}
