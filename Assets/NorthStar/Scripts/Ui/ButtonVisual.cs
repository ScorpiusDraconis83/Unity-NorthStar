// Copyright (c) Meta Platforms, Inc. and affiliates.
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NorthStar
{
    /// <summary>
    /// Button hover/click sounds and tween effects
    /// </summary>
    public class ButtonVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float m_hoverScale = 1f;
        [SerializeField] private float m_hoverTweenTime = .1f;
        [SerializeField] private float m_selectScale = 1f;
        [SerializeField] private SoundPlayer m_enterSounds;
        [SerializeField] private SoundPlayer m_exitSounds;
        [SerializeField] private SoundPlayer m_selectSounds;
        [SerializeField] private Ease m_ease;
        private void TweenScale(float scale, float time)
        {
            _ = transform.DOKill(true);
            _ = transform.DOScale(scale, time).SetEase(m_ease);
        }
        private void OnDisable()
        {
            _ = transform.DOKill(true);
            TweenScale(1, m_hoverTweenTime);
            _ = transform.DOKill(true);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_enterSounds?.Play();
            TweenScale(m_hoverScale, m_hoverTweenTime);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            m_exitSounds?.Play();
            TweenScale(1, m_hoverTweenTime);
        }
    }
}
