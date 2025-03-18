// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Fades audio sources
    /// </summary>
    public class FadeVolume : MonoBehaviour
    {
        [SerializeField] private AudioSource m_audioSource;
        [SerializeField] private float m_volumeOnFadeIn;
        [SerializeField] private bool m_destroyAfterFadeOut = false;
        [SerializeField] private bool m_fadeInAudioOnStart = false;
        [SerializeField] private float m_startFadeInTime = 1f;

        private void Start()
        {
            if (m_fadeInAudioOnStart) FadeInAudio(m_startFadeInTime);
        }
        public void FadeOutAudio(float time)
        {
            if (m_audioSource is null)
            {
                Debug.Log("There is no audiosource specified, please add one to " + gameObject);
                return;
            }
            _ = m_audioSource.DOFade(0f, time);
            _ = m_audioSource.DOFade(0f, time);
            if (m_destroyAfterFadeOut)
            {
                _ = StartCoroutine(DestroyAfterWait(time));
            }
        }

        private IEnumerator DestroyAfterWait(float time)
        {
            yield return new WaitForSeconds(time);
            Destroy(gameObject);
        }

        public void FadeInAudio(float time)
        {
            if (m_audioSource is null)
            {
                Debug.Log("There is no audiosource specified, please add one to " + gameObject);
                return;
            }
            m_audioSource.volume = 0f;
            _ = m_audioSource.DOFade(m_volumeOnFadeIn, time);
        }

        [ContextMenu("Fade out over one second")]
        private void FadeOverOneSecond()
        {
            FadeOutAudio(1f);
            if (m_destroyAfterFadeOut)
            {
                _ = StartCoroutine(DestroyAfterWait(1f));
            }
        }
    }
}
