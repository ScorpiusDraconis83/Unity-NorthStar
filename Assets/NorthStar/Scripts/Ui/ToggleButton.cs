// Copyright (c) Meta Platforms, Inc. and affiliates.
using DG.Tweening;
using Meta.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NorthStar
{
    /// <summary>
    /// Animated toggle button
    /// </summary>
    public class ToggleButton : MonoBehaviour
    {
        [SerializeField, AutoSet] private Button m_button;
        [SerializeField] private Image m_bgImage, m_buttonImage;
        [SerializeField] private float m_tweenTime;
        [SerializeField] private Color m_onColour, m_offColour;
        [SerializeField] private float m_xPos;
        [SerializeField] private Ease m_ease;
        [SerializeField] private bool m_state;
        public UnityEvent<bool> OnToggle;

        public bool State { get => m_state; set { m_state = value; SyncVisual(); } }

        private void Start()
        {
            SyncVisual();
            KillTweens(true);
        }

        private void OnValidate()
        {
            SyncVisual();
        }

        private void SyncVisual()
        {
            KillTweens();
            if (m_state) { ToggleOn(); }
            else { ToggleOff(); }
        }

        [ContextMenu("Toggle")]
        public void Toggle()
        {
            m_state = !m_state;
            SyncVisual();
            OnToggle.Invoke(m_state);
        }

        private void ToggleOn()
        {
            _ = m_bgImage.DOColor(m_onColour, m_tweenTime).SetEase(m_ease);
            _ = m_buttonImage.DOColor(m_onColour, m_tweenTime).SetEase(m_ease);
            _ = m_buttonImage.transform.DOLocalMoveX(m_xPos, m_tweenTime).SetEase(m_ease);
        }

        private void ToggleOff()
        {
            _ = m_bgImage.DOColor(m_offColour, m_tweenTime).SetEase(m_ease);
            _ = m_buttonImage.DOColor(m_offColour, m_tweenTime).SetEase(m_ease);
            _ = m_buttonImage.transform.DOLocalMoveX(-m_xPos, m_tweenTime).SetEase(m_ease);
        }

        private void KillTweens(bool complete = false)
        {
            _ = m_bgImage.DOKill(complete);
            _ = m_buttonImage.DOKill(complete);
            _ = m_buttonImage.transform.DOKill(complete);
        }

    }
}
