// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Manages current active menu and menu placement relative to the player
    /// </summary>
    public class Menus : MonoBehaviour
    {
        public enum MenuOptions
        {
            None = -1,
            Pause,
            Settings,
            Debug,
        }
        [SerializeField] private UiMenu[] m_menus;
        [SerializeField] private MenuOptions m_defaultMenu;

        [SerializeField] private float m_placementDistance = 1.0f;
        [SerializeField] private float m_placementHeightOffset = -0.25f;
        public MenuOptions CurrentMenu = MenuOptions.None;

        public void SetMenu(int menuOption)
        {
            for (var i = 0; i < m_menus.Length; i++)
            {
                if (i == menuOption) m_menus[i].Open();
                else m_menus[i].Close();
            }
            CurrentMenu = (MenuOptions)menuOption;
        }

        public void OrientMenu()
        {
            var cameraTransform = Camera.main.transform;
            var lateralForward = cameraTransform.forward;
            lateralForward.y = 0;
            lateralForward.Normalize();

            transform.position = cameraTransform.position + lateralForward * m_placementDistance + Vector3.up * m_placementHeightOffset;
            transform.rotation = Quaternion.LookRotation(lateralForward, Vector3.up);
        }

        public void SetMenu(MenuOptions menuOption)
        {
            SetMenu((int)menuOption);
        }

        private void Update()
        {
            if (OVRInput.GetDown(OVRInput.Button.Start))
            {
                if (CurrentMenu != MenuOptions.None)
                {
                    SetMenu(MenuOptions.None);
                }
                else
                {
                    SetMenu(m_defaultMenu);
                    OrientMenu();
                }
            }
        }
    }
}
