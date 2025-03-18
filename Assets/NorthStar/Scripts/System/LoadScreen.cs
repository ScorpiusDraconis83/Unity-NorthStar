// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections;
using Meta.Utilities.Narrative;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NorthStar
{
    /// <summary>
    /// Manages the loading screen logic and visuals
    /// </summary>
    public class LoadScreen : MonoBehaviour
    {
        private static LoadScreen s_instance;

        public static LoadScreen Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new GameObject("LoadScreen").AddComponent<LoadScreen>();
                    DontDestroyOnLoad(s_instance.gameObject);
                }
                return s_instance;
            }
        }

        private bool m_loading = false;

        public void Load(string nextScene, bool resetGame = false)
        {
            if (m_loading) return;
            StopAllCoroutines();
            _ = StartCoroutine(LoadScene(nextScene, resetGame));
        }

        public void LoadWithTaskID(string nextScene, TaskID id)
        {
            if (m_loading) return;
            StopAllCoroutines();
            _ = StartCoroutine(LoadSceneWithTaskID(nextScene, id));
        }

        public void GoToCredits()
        {
            if (m_loading) return;
            StopAllCoroutines();
            _ = StartCoroutine(LoadCredits());
        }

        private IEnumerator LoadCredits()
        {
            m_loading = true;
            _ = ScreenFader.Instance.DoFadeOut(.1f);
            yield return new WaitForSeconds(.1f);
            GameFlowController.Instance.ResetLoadState();
            SceneManager.LoadScene("CreditsScene");
            m_loading = false;
        }

        private IEnumerator LoadScene(string nextScene, bool resetGame)
        {
            m_loading = true;
            _ = ScreenFader.Instance.DoFadeOut(.1f);
            yield return new WaitForSeconds(.1f);
            GameFlowController.Instance.ResetLoadState();
            SceneManager.LoadScene("LoadScene");
            yield return null;
            var operation = SceneManager.LoadSceneAsync(nextScene);
            operation.allowSceneActivation = false;
            yield return new WaitForSeconds(5);
            _ = ScreenFader.Instance.DoFadeOut(.1f);
            yield return null;
            while (operation.progress < .9f)
            {
                yield return null;
            }
            yield return new WaitForSeconds(.1f);
            operation.allowSceneActivation = true;
            m_loading = false;
            if (resetGame)
            {
                yield return null;
                TaskManager.StartNarrative();
            }
        }


        private IEnumerator LoadSceneWithTaskID(string nextScene, TaskID iD)
        {
            m_loading = true;
            _ = ScreenFader.Instance.DoFadeOut(.1f);
            yield return new WaitForSeconds(.1f);
            GameFlowController.Instance.ResetLoadState();
            SceneManager.LoadScene("LoadScene");
            yield return null;
            var operation = SceneManager.LoadSceneAsync(nextScene);
            operation.allowSceneActivation = false;
            yield return new WaitForSeconds(5);
            _ = ScreenFader.Instance.DoFadeOut(.1f);
            yield return null;
            while (operation.progress < .9f)
            {
                yield return null;
            }
            yield return new WaitForSeconds(.1f);
            operation.allowSceneActivation = true;
            m_loading = false;

            yield return null;
            TaskManager.StartNarrativeFromTaskID(iD);
        }
    }
}
