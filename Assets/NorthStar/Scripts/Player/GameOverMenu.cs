// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NorthStar
{
    public class GameOverMenu : MonoBehaviour
    {
        [SerializeField] private Transform m_canvas;
        [SerializeField] private Transform m_contentRoot;

        [SerializeField] private float m_placementDistance = 1.0f;
        [SerializeField] private float m_placementHeightOffset = -0.25f;

        [SerializeField] private Button m_creditsButton;
        [SerializeField] private Button m_restartButton;
        [SerializeField] private TextMeshProUGUI m_titleText;

        [SerializeField] private CanvasGroup m_canvasGroup;

        [SerializeField] private float m_fadeTime = 0.5f;
        [SerializeField] private float m_autoRestartTime = 3f;

        private bool m_restarting;
        private float m_restartTimeRemaining;

        private void Awake()
        {
            if (Application.isPlaying && GameFlowController.Instance)
            {
                SetVisible(false);
                GameFlowController.Instance.GameOverRequested.AddListener(OnGameOverRequested);
            }
        }

        private void OnDestroy()
        {
            if (Application.isPlaying && GameFlowController.Instance)
            {
                GameFlowController.Instance.GameOverRequested.RemoveListener(OnGameOverRequested);
            }
        }

        private void OnGameOverRequested()
        {
            SetVisible(true);
            _ = StartCoroutine(FadeCoroutine(0, 1));
        }

        private void SetVisible(bool visible)
        {
            if (!Application.isPlaying) return;
            if (m_canvas.gameObject.activeSelf == visible) return;

            m_canvas.gameObject.SetActive(visible);

            if (visible)
            {
                m_restarting = false;
                m_restartTimeRemaining = m_autoRestartTime;
                m_creditsButton.gameObject.SetActive(true);
                m_titleText.SetText("Game Over"); // TODO: localise

                var cameraTransform = Camera.main.transform;
                var lateralForward = cameraTransform.forward;
                lateralForward.y = 0;
                lateralForward.Normalize();

                transform.position = cameraTransform.position + lateralForward * m_placementDistance + Vector3.up * m_placementHeightOffset;
                transform.rotation = Quaternion.LookRotation(lateralForward, Vector3.up);
            }
        }

        private void Update()
        {
            if (!m_restarting && m_canvas.gameObject.activeSelf && m_restartTimeRemaining > 0)
            {
                m_restartTimeRemaining -= Time.deltaTime;
                if (m_restartTimeRemaining <= 0) RestartGame();
            }
        }

        #region Button Targets

        public void ShowCredits()
        {
            m_creditsButton.gameObject.SetActive(false);
            m_titleText.SetText("Credits"); // TODO: localise
            _ = StartCoroutine(ShowCreditsCoroutine());
        }

        public void RestartGame()
        {
            if (m_restarting) return;
            m_restarting = true;

            StopAllCoroutines();
            _ = StartCoroutine(RestartGameCoroutine());
        }

        #endregion

        #region Coroutines

        private IEnumerator FadeCoroutine(float from, float to, bool forceEntireRange = true)
        {
            // if forceEntireRange is false, use an inverse lerp to decide how much time the remaining fade will take
            // this ensures that we can safely start a fade out part way through a fade in
            var t = forceEntireRange ? 0 : Mathf.InverseLerp(from, to, m_canvasGroup.alpha) * m_fadeTime;

            while (t < m_fadeTime)
            {
                t += Time.deltaTime;
                m_canvasGroup.alpha = Mathf.Lerp(from, to, t / m_fadeTime);
                yield return null;
            }

            m_canvasGroup.alpha = to;
        }

        private IEnumerator RestartGameCoroutine()
        {
            yield return FadeCoroutine(1, 0, false);
            GameFlowController.Instance.RestartGame();
        }

        private IEnumerator ShowCreditsCoroutine()
        {
            // TODO: show credits, etc.
            yield break;
        }

        #endregion
    }
}