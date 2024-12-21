// Copyright (c) Meta Platforms, Inc. and affiliates.
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace NorthStar
{
    /// <summary>
    /// Cycles between several images in the UI, used for first time user experience (FTUE) tutorials
    /// </summary>
    public class SlideShow : MonoBehaviour
    {
        [SerializeField] private Image[] m_imgs;
        [SerializeField] private CanvasGroup[] m_canvasGroups;

        [SerializeField] private float m_crossfadeTime = .1f;
        [SerializeField] private Sprite[] m_sprites;

        [SerializeField] private float m_cycleTime = .5f;
        private float m_timer = 0;
        private int m_index = 0;

        private int m_inactiveImage = 0;

        private void Start()
        {
            foreach (var item in m_imgs)
            {
                item.sprite = m_sprites[0];
            }
            GoNext();
        }

        private void Update()
        {
            m_timer += Time.deltaTime;
            if (m_timer > m_cycleTime)
            {
                m_timer = 0;
                GoNext();
            }
        }

        [ContextMenu("Next")]
        private void GoNext()
        {
            m_imgs[m_inactiveImage].sprite = m_sprites[m_index];
            m_index++;
            if (m_index >= m_imgs.Length)
                m_index = 0;
            _ = m_canvasGroups[m_inactiveImage].DOFade(1, m_crossfadeTime).SetEase(Ease.Linear);
            m_inactiveImage = m_inactiveImage == 0 ? 1 : 0;
            _ = m_canvasGroups[m_inactiveImage].DOFade(0, m_crossfadeTime).SetEase(Ease.Linear);
        }
    }
}
