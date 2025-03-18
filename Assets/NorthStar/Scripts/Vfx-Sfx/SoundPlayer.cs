// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using Meta.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace NorthStar
{
    /// <summary>
    /// Simple yet versatile component for playing sounds with randomized variations from scripts or events
    /// </summary>
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField, AutoSet] private AudioSource m_source;
        [SerializeField] public List<AudioClip> Clips;
        [SerializeField] private float m_pitchRange = 0, m_volumeRange;
        [SerializeField] private bool m_playOnAwake = false;
        [SerializeField] private bool m_oneShotMode = false;
        [HideInInspector] public float BaseVolume;
        [SerializeField] private bool m_allowOverlap = false;

        private bool m_isPlaying;
        private float m_defaultPitch;

        public bool IsPlaying { get => m_isPlaying; set { } }

        private void Awake()
        {
            m_defaultPitch = m_source.pitch;
            m_source.playOnAwake = false;
            BaseVolume = m_source.volume;
        }

        private void Start()
        {
            if (m_playOnAwake)
                Play();
        }

        private void OnValidate()
        {
            if (m_source == null)
            {
                SetupInEditor();
            }
        }

        public void SetupInEditor()
        {
            if (GetComponent<AudioSource>() == null)
            {
                _ = gameObject.AddComponent<AudioSource>();
            }
            m_source = gameObject.GetComponent<AudioSource>();
            m_source.playOnAwake = false;
            m_source.spatialize = true;
            m_source.spatialBlend = 1;
            if (GetComponent<MetaXRAudioSource>() == null)
            {
                var xrSource = gameObject.AddComponent<MetaXRAudioSource>();
                xrSource.GainBoostDb = 10;
            }

        }

        public void Play()
        {
            if (m_isPlaying && !m_allowOverlap) return;
            if (Clips.Count == 0) return;
            enabled = true;
            m_isPlaying = true;
            var index = Random.Range(0, Clips.Count);
            m_source.clip = Clips[index];
            m_source.pitch = Mathf.Max(m_defaultPitch + Random.Range(-1f, 1f) * m_pitchRange, 0.0f);
            m_source.volume = Mathf.Max(BaseVolume + Random.Range(-1f, 1f) * m_volumeRange, 0.0f);
            if (m_oneShotMode)
                m_source.PlayOneShot(Clips[index], Mathf.Max(BaseVolume + Random.Range(-1f, 1f) * m_volumeRange, 0.0f));
            else
                m_source.Play();
        }

        public void Stop()
        {
            if (!m_isPlaying) return;
            m_isPlaying = false;
            m_source.Stop();
        }

        private void Update()
        {
            if (m_isPlaying)
            {
                if (!m_source.isPlaying)
                    Stop();
            }

            if (!m_isPlaying)
            {
                enabled = false;
            }
        }
    }
}
