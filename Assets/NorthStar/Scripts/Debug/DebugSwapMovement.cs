// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class DebugSwapMovement : MonoBehaviour
    {
        public GrabMovement GrabMovement;
        public LeverInteractable Interactable;

        private bool m_triggered;

        public void Update()
        {
            var lVal = Interactable.Value;
            if (lVal > .9f && !m_triggered)
            {
                if (GrabMovement.MoveMode == GrabMovement.MoveModes.Linear)
                {
                    GrabMovement.MoveMode = GrabMovement.MoveModes.Snap;
                }
                else if (GrabMovement.MoveMode == GrabMovement.MoveModes.Snap)
                {
                    GrabMovement.MoveMode = GrabMovement.MoveModes.Linear;
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
