// Copyright (c) Meta Platforms, Inc. and affiliates.
using TMPro;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Handler for debug watches, simply evaluates the lambda as a string and displays the result via a text label in the UI
    /// </summary>
    public class DebugWatchHandler : MonoBehaviour
    {
        public DebugSystem.DebugWatch ConnectedValue
        {
            get => m_connectedValue;

            set
            {
                m_connectedValue = value;
                UpdateUI();
            }
        }

        [SerializeField] private TextMeshProUGUI m_nameLabel;
        [SerializeField] private TextMeshProUGUI m_valueLabel;

        private DebugSystem.DebugWatch m_connectedValue;

        private void Update()
        {
            UpdateUI();
        }


        private void UpdateUI()
        {
            if (m_connectedValue != null)
            {
                m_nameLabel.text = m_connectedValue.Name;
                m_valueLabel.text = m_connectedValue.ReadValue();
            }
        }
    }
}
