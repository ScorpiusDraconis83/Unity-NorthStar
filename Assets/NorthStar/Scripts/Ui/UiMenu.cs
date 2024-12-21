// Copyright (c) Meta Platforms, Inc. and affiliates.
using DG.Tweening;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Generic menu controller with support for fading in and out
    /// </summary>
    public class UiMenu : MonoBehaviour
    {
        private bool m_open = true;
        [SerializeField] private CanvasGroup m_canvasGroup;
        [SerializeField] private GameObject m_parentGo;
        [SerializeField] private float m_fadeInOutTime = .1f;
        [SerializeField] private bool m_openOnButtonPress;
        [SerializeField] private bool m_closeByDefault = true;

        private void Start()
        {
            if (m_closeByDefault)
            {
                Close();
                _ = m_canvasGroup.DOKill(true);
            }
            OnStart();
        }

        protected virtual void OnStart() { }

        [ContextMenu("Open")]
        public void Open()
        {
            if (m_open) return;
            m_open = true;
            m_parentGo.SetActive(true);
            _ = m_canvasGroup.DOKill();
            _ = m_canvasGroup.DOFade(1, m_fadeInOutTime);
            OnOpen();
        }

        protected virtual void OnOpen() { }

        [ContextMenu("Close")]
        public void Close()
        {
            if (!m_open) return;
            m_open = false;
            _ = m_canvasGroup.DOKill();
            _ = m_canvasGroup.DOFade(0, m_fadeInOutTime).onComplete += () => { m_parentGo.SetActive(false); };
            OnClose();
        }

        protected virtual void OnClose() { }

        private void Update()
        {
            if (m_openOnButtonPress && OVRInput.GetDown(OVRInput.Button.Start))
            {
                if (!m_open)
                    Open();
                else Close();
            }
        }
    }
}
