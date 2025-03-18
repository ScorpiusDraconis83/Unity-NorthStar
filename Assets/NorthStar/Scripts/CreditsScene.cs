// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections;
using Meta.Utilities.Narrative;
using UnityEngine;
using UnityEngine.UI;

namespace NorthStar
{
    /// <summary>
    /// Handles loading back into the first scene after credits are complete
    /// </summary>
    public class CreditsScene : MonoBehaviour
    {
        public TaskID FirstTask = TaskID.None;
        [SerializeField] private Button m_button;
        private void Start()
        {
            GameFlowController.Instance.PreloadScene("Beat1");
            m_button.gameObject.SetActive(false);
        }

        public void ReloadGame()
        {
            _ = StartCoroutine(Reload());
        }

        private void Update()
        {
            m_button.gameObject.SetActive(GameFlowController.Instance.GetLoadProgress() >= .9f);
        }

        private IEnumerator Reload()
        {
            _ = ScreenFader.Instance.DoFadeOut(.1f);
            yield return new WaitForSeconds(.1f);
            GameFlowController.Instance.CompleteSceneLoad("Beat1");
            TaskManager.StartNarrativeFromTaskID(FirstTask);
        }
    }
}
