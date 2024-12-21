// Copyright (c) Meta Platforms, Inc. and affiliates.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NorthStar
{
    /// <summary>
    /// Handler for int debug values, uses a slider UI
    /// </summary>
    public class DebugIntValueHandler : MonoBehaviour
    {
        public DebugSystem.IntValue ConnectedValue
        {
            get => m_connectedValue;

            set
            {
                m_connectedValue = value;
                m_slider.minValue = m_connectedValue.Min;
                m_slider.maxValue = m_connectedValue.Max;
                m_slider.value = m_connectedValue.Value;
                m_slider.onValueChanged.RemoveAllListeners();
                m_slider.onValueChanged.AddListener(OnValueChanged);
                m_connectedValue.OnSet.AddListener(OnConnectedValueChanged);
                UpdateUI();
            }
        }

        private DebugSystem.IntValue m_connectedValue;

        [SerializeField] private TextMeshProUGUI m_nameLabel;
        [SerializeField] private TextMeshProUGUI m_valueLabel;
        [SerializeField] private Slider m_slider;
        [SerializeField] private string m_formatString = "{0}";

        private void OnValueChanged(float value)
        {
            m_connectedValue.Value = (int)value;
        }

        private void OnConnectedValueChanged(int value)
        {
            m_slider.value = value;
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
                m_valueLabel.text = string.Format(m_formatString, m_connectedValue.Value);
            }
        }
    }
}
