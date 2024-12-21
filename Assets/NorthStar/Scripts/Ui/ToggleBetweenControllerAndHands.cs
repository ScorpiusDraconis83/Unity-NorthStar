// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Automatically toggles between using hands or controllers as input
    /// </summary>
    public class ToggleBetweenControllerAndHands : MonoBehaviour
    {
        [SerializeField] private GameObject m_hands, m_controllers;
        private void Update()
        {
            var usingHands = OVRInput.GetControllerIsInHandState(OVRInput.Hand.HandLeft) == OVRInput.ControllerInHandState.ControllerNotInHand;

            m_hands.SetActive(usingHands);
            m_controllers.SetActive(!usingHands);
        }
    }
}
