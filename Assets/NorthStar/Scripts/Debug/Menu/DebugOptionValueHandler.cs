// Copyright (c) Meta Platforms, Inc. and affiliates.
using TMPro;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Handler for option values, essentially an enum. Uses a dropdown UI
    /// </summary>
    public class DebugOptionValueHandler : MonoBehaviour
    {
        public DebugSystem.OptionsValue ConnectedValue
        {
            get => m_connectedValue;

            set
            {
                m_connectedValue = value;
                m_dropdown.value = m_connectedValue.Value;
                m_dropdown.onValueChanged.RemoveAllListeners();
                m_dropdown.onValueChanged.AddListener(OnValueChanged);
                m_connectedValue.OnSet.AddListener(OnConnectedValueChanged);
                UpdateUI();
            }
        }

        private DebugSystem.OptionsValue m_connectedValue;

        [SerializeField] private TextMeshProUGUI m_nameLabel;
        [SerializeField] private TMP_Dropdown m_dropdown;

        private void OnValueChanged(int value)
        {
            m_connectedValue.Value = value;
        }

        private void OnConnectedValueChanged(int value)
        {
            m_dropdown.value = value;
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
                m_dropdown.ClearOptions();
                foreach (var option in m_connectedValue.Options)
                {
                    m_dropdown.options.Add(new TMP_Dropdown.OptionData(option));
                }
            }
        }
    }
}
