// Copyright (c) Meta Platforms, Inc. and affiliates.
using DG.Tweening;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Tells the gameflow to load the scene after a screenfade and a timer
    /// </summary>
    public class EndSceneLoadAfterTime : MonoBehaviour
    {
        [SerializeField] private float m_fadeTime = .2f;
        [SerializeField] private float m_fadeHoldTime = 0f;
        [SerializeField] private string m_sceneName = "";
        public void Trigger()
        {
            var sequence = DOTween.Sequence()
                .Append(ScreenFader.Instance.DoFadeOut(m_fadeTime))
                .AppendInterval(m_fadeHoldTime)
                .AppendCallback(() => { GameFlowController.Instance.CompleteSceneLoad(m_sceneName); });
        }
    }
}
