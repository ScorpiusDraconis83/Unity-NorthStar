// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// A simple effect component with sounds and particles that can be driven via scripts and events
    /// </summary>
    public class EffectObject : MonoBehaviour
    {
        private ParticleSystem[] m_particles;
        private SoundPlayer m_soundPlayer;
        private bool m_hasSound;
        private float m_baseVolume;

        private void Awake()
        {
            m_particles = GetComponentsInChildren<ParticleSystem>();
            m_hasSound = TryGetComponent(out m_soundPlayer);
            if (m_hasSound)
                m_baseVolume = GetComponent<AudioSource>().volume;
        }

        public void Play(float intensity = 1)
        {
            if (m_particles.Length > 0)
            {
                var index = Random.Range(0, m_particles.Length);
                m_particles[index].Play();
            }
            if (m_hasSound)
            {
                m_soundPlayer.BaseVolume = m_baseVolume * intensity;
                m_soundPlayer.Play();
            }
        }

        public void Stop()
        {
            foreach (var particle in m_particles)
            {
                particle.Stop();
            }
            if (m_hasSound) m_soundPlayer.Stop();
        }

    }
}
