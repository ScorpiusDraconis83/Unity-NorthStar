// Copyright (c) Meta Platforms, Inc. and affiliates.
using DG.Tweening;
using Meta.Utilities;
using TMPro;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Displays text above registered characters
    /// </summary>
    public class Subtitle : MonoBehaviour
    {
        [CharacterDropdown] public string Id;
        [SerializeField, HideInInspector] private CharacterManager m_characterManager;
        [SerializeField, HideInInspector] private TextMeshProUGUI m_text;
        [SerializeField, AutoSet] private CanvasGroup m_canvasGroup;
        [SerializeField, AutoSet] private FloatingText m_floatingText;
        private float m_timeLastDisplayed;
        private bool m_shown;
        private void OnValidate()
        {
            m_characterManager = FindAnyObjectByType<CharacterManager>();
            m_text = GetComponentInChildren<TextMeshProUGUI>();
        }
        private void OnEnable()
        {
            m_characterManager.RegisterSubtitleObject(this);
            m_canvasGroup.alpha = 0;
        }
        private void OnDisable()
        {
            m_characterManager.DeRegisterSubtitleObject(this);
        }

        private void Update()
        {
            if (m_shown && Time.time - m_timeLastDisplayed > GlobalSettings.ScreenSettings.TextShowTime)
            {
                m_shown = false;
                _ = DOTween.Kill(m_canvasGroup);
                _ = m_canvasGroup.DOFade(0, GlobalSettings.ScreenSettings.TextFadeTime);
            }
        }

        public void DisplayText(TextObject text)
        {
            if (GlobalSettings.PlayerSettings.DisableCaptions)
                return;
            m_text.text = text.Text;
            if (!m_shown)
            {
                m_shown = true;
                _ = DOTween.Kill(m_canvasGroup);
                _ = m_canvasGroup.DOFade(1, GlobalSettings.ScreenSettings.TextFadeTime);
            }
            m_timeLastDisplayed = Time.time;
            m_floatingText.SyncPosition();
        }
    }
}
