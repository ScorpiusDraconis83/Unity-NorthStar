// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Text;
using TMPro;
using UnityEngine;

namespace NorthStar
{
    public class RopeSystemTester : MonoBehaviour
    {
        [SerializeField] private RopeSystem m_ropeSystem;

        [SerializeField] private TextMeshProUGUI m_text;

        private StringBuilder m_textBuffer = new();

        private bool m_UpdateText = true;
        private bool m_Tied = false;
        private float m_TotalAmountSpooled = 0;
        private float m_TotalSlackPercentage = 0;
        private int m_TotalBendAnchors = 0;
        private float m_TotalBendRevolutions = 0;

        private void Update()
        {
            if (m_Tied != m_ropeSystem.Tied)
            {
                m_Tied = m_ropeSystem.Tied;
                m_UpdateText = true;
            }

            if (Mathf.Abs(m_TotalAmountSpooled - m_ropeSystem.TotalAmountSpooled) > 0.005)
            {
                m_TotalAmountSpooled = m_ropeSystem.TotalAmountSpooled;
                m_UpdateText = true;
            }

            if (Mathf.Abs(m_TotalSlackPercentage - m_ropeSystem.TotalSlackPercentage) > 0.005)
            {
                m_TotalSlackPercentage = m_ropeSystem.TotalSlackPercentage;
                m_UpdateText = true;
            }

            if (m_TotalBendAnchors != m_ropeSystem.TotalBendAnchors)
            {
                m_TotalBendAnchors = m_ropeSystem.TotalBendAnchors;
                m_UpdateText = true;
            }

            if (Mathf.Abs(m_TotalBendRevolutions - m_ropeSystem.TotalBendRevolutions) > 0.005)
            {
                m_TotalBendRevolutions = m_ropeSystem.TotalBendRevolutions;
                m_UpdateText = true;
            }

            if (m_UpdateText)
            {
                var c = m_Tied ? "green" : "red";

                _ = m_textBuffer.Clear()
                    .AppendFormat("Tied: <color=\"{0}\">{1}</color>\n", c, m_Tied)
                    .AppendFormat("Spooled: {0:F2}%\n", m_TotalAmountSpooled * 100)
                    .AppendFormat("Slack: {0:F2}%\n", m_TotalSlackPercentage * 100)
                    .AppendFormat("Bends: {0}\n", m_TotalBendAnchors)
                    .AppendFormat("Revolutions: {0:F1}\n", m_TotalBendRevolutions);
                m_text.SetText(m_textBuffer);
                m_UpdateText = false;
            }
        }
    }
}
