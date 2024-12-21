// Copyright (c) Meta Platforms, Inc. and affiliates.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NorthStar
{
    /// <summary>
    /// Handler for debug actions, which are just buttons that activate some custom code
    /// </summary>
    public class DebugActionHandler : MonoBehaviour
    {
        public DebugSystem.DebugAction ConnectedValue
        {
            get => m_connectedValue;

            set
            {
                m_connectedValue = value;
                m_button.onClick.RemoveAllListeners();
                m_button.onClick.AddListener(OnClick);
                UpdateUI();
            }
        }

        [SerializeField] private TextMeshProUGUI m_nameLabel;
        [SerializeField] private Button m_button;

        private DebugSystem.DebugAction m_connectedValue;

        private void OnClick()
        {
            m_connectedValue.Action.Invoke();
        }

        private void UpdateUI()
        {
            if (m_connectedValue != null)
            {
                m_nameLabel.text = m_connectedValue.Name;
            }
        }
    }
}
