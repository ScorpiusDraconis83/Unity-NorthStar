// Copyright (c) Meta Platforms, Inc. and affiliates.
using System;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Simple class to expose events for the touch controllers
    /// </summary>
    public class ControllerMappings : MonoBehaviour
    {
        public Action LeftStickForward;
        public Action RightStickForward;
        public Action LeftStickRelesed;
        public Action RightStickRelesed;

        private bool m_leftStickEngaged = false;
        private bool m_rightStickEngaged = false;
        private void Update()
        {
            var lControllerDir = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
            if (!m_leftStickEngaged && lControllerDir.y > 0)
            {
                m_leftStickEngaged = true;
                LeftStickForward?.Invoke();
            }
            else if (m_leftStickEngaged && lControllerDir.y <= 0)
            {
                m_leftStickEngaged = false;
                LeftStickRelesed?.Invoke();
            }
            var rControllerDir = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
            if (!m_rightStickEngaged && rControllerDir.y > 0)
            {
                m_rightStickEngaged = true;
                RightStickForward?.Invoke();
            }
            else if (m_rightStickEngaged && rControllerDir.y <= 0)
            {
                m_rightStickEngaged = false;
                RightStickRelesed?.Invoke();
            }
        }

        public bool UsingControler()
        {
            return OVRInput.IsControllerConnected(OVRInput.Controller.Touch);
        }
    }
}
