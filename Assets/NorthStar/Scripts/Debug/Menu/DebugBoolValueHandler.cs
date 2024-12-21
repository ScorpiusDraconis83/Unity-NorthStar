// Copyright (c) Meta Platforms, Inc. and affiliates.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NorthStar
{
    /// <summary>
    /// Handler for boolean debug values, uses a checkbox UI
    /// </summary>
    public class DebugBoolValueHandler : MonoBehaviour
    {
        public DebugSystem.BoolValue ConnectedValue
        {
            get => m_connectedValue;

            set
            {
                m_connectedValue = value;
                m_toggle.isOn = m_connectedValue.Value;
                m_toggle.onValueChanged.AddListener(OnValueChanged);
                m_connectedValue.OnSet.AddListener(OnConnectedValueChanged);
                UpdateUI();
            }
        }

        private DebugSystem.BoolValue m_connectedValue;

        [SerializeField] private TextMeshProUGUI m_nameLabel;
        [SerializeField] private Toggle m_toggle;

        private void OnValueChanged(bool value)
        {
            m_connectedValue.Value = value;
        }

        private void OnConnectedValueChanged(bool value)
        {
            m_toggle.isOn = value;
            UpdateUI();
        }

        private void OnDisable()
        {
            if (m_connectedValue != null)
            {
                m_connectedValue.OnSet.RemoveListener(OnConnectedValueChanged);
            }
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
