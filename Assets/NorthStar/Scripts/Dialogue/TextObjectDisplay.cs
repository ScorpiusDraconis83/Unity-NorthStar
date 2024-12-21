// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using TMPro;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Writes text to tmp objects from text assets
    /// </summary>
    public class TextObjectDisplay : MonoBehaviour
    {
        [SerializeField, AutoSet] private TextMeshProUGUI m_text;
        [SerializeField] private TextObject m_textObject;

        public void SetTextObject(TextObject textObject)
        {
            if (textObject is null)
                return;
            m_textObject = textObject;
            m_text.text = m_textObject.Text;
        }

        private void Start()
        {
            TextManager.Instance.OnReload += Reload;
            Reload();
        }
        private void OnDestroy()
        {
            TextManager.Instance.OnReload -= Reload;
        }

        private void Reload()
        {
            if (m_textObject == null)
                return;
            m_text.text = m_textObject.Text;
        }

        private void OnValidate()
        {
            Reload();
        }
    }
}
