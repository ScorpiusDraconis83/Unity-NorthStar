// Copyright (c) Meta Platforms, Inc. and affiliates.
using TMPro;
using UnityEngine;

namespace NorthStar
{
    public class DebugInteractionReadout : MonoBehaviour
    {
        public BaseJointInteractable<float> Interactable;
        public TMP_Text Text;

        public void Update()
        {
            Text.text = Interactable.Value.ToString("0.00");
        }
    }
}
