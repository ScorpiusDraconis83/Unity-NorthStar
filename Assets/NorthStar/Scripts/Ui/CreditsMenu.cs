// Copyright (c) Meta Platforms, Inc. and affiliates.
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NorthStar
{
    /// <summary>
    /// Controller for the credits
    /// </summary>
    public class CreditsMenu : UiMenu
    {
        [SerializeField] private TextAsset m_creditsTextSource;
        [SerializeField] private TextMeshProUGUI m_creditsText;

        [SerializeField] private float m_scrollSpeed;
        [SerializeField] private RectTransform m_scrollElement;
        [SerializeField] private float m_distance = 1000;
        [SerializeField] private bool m_startScrollOnOpen = true;
        public UnityEvent OnEndScroll;
        private bool m_scrolling;

        [ContextMenu("Update credits text")]
        protected void Awake()
        {
            m_creditsText.text = m_creditsTextSource.text;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(m_creditsText);
#endif
        }

        protected override void OnStart()
        {
            if (m_startScrollOnOpen)
            {
                StartScroll();
            }
        }

        [ContextMenu("Start")]
        public void StartScroll()
        {
            if (m_scrolling)
                return;

            var lastElement = m_scrollElement.GetChild(m_scrollElement.childCount - 1);
            m_scrolling = true;
            var time = m_distance / m_scrollSpeed;
            _ = m_scrollElement.DOAnchorPosY(m_scrollElement.anchoredPosition.y + m_distance, time).SetEase(Ease.Linear).onComplete += OnComplete;
        }

        private void OnComplete()
        {
            m_scrolling = false;
            OnEndScroll.Invoke();
        }
    }
}
