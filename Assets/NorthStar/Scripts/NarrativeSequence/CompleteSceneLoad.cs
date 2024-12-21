// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class CompleteSceneLoad : MonoBehaviour
    {
        [SerializeField] private string m_sceneName;
        public void Execute()
        {
            if (GameFlowController.Instance is not null)
            {
                GameFlowController.Instance.CompleteSceneLoad(m_sceneName);
            }
        }
    }
}
