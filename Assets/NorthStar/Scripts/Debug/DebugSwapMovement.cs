// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class DebugSwapMovement : MonoBehaviour
    {
        public GrabMovement grabMovement;
        public LeverInteractable interactable;

        private bool m_triggered;

        public void Update()
        {
            var lVal = interactable.Value;
            if (lVal > .9f && !m_triggered)
            {
                if (grabMovement.MoveMode == GrabMovement.MoveModes.Linear)
                {
                    grabMovement.MoveMode = GrabMovement.MoveModes.Snap;
                }
                else if (grabMovement.MoveMode == GrabMovement.MoveModes.Snap)
                {
                    grabMovement.MoveMode = GrabMovement.MoveModes.Linear;
                }
                m_triggered = true;
            }
            if (lVal < .2f && m_triggered)
            {
                m_triggered = false;
            }
        }
    }
}
