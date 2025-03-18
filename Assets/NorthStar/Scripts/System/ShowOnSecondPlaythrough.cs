// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace NorthStar
{
    public class ShowOnSecondPlaythrough : MonoBehaviour
    {
        private void Start()
        {
            gameObject.SetActive(GameFlowController.Instance.GameCompleteOnce);
        }
    }
}
