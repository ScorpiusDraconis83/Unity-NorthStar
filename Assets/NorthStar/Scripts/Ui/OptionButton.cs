// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Events;

namespace NorthStar
{
    /// <summary>
    /// Button that automatically cycles through set of options on press
    /// </summary>
    public class OptionButton : MonoBehaviour
    {
        [SerializeField] private TextObject[] m_textAssets;
        [SerializeField] private TextObjectDisplay m_display;
        [SerializeField] private int m_index;
        public int Index { get => m_index; set => SetIndex(value); }
        public UnityEvent<int> OnValueChanged;

        private void Start()
        {
            SetIndex(m_index);
        }

        public void SetIndex(int index)
        {
            if (index > m_textAssets.Length - 1)
                index = 0;
            if (index < 0)
                index = m_textAssets.Length - 1;
            m_display.SetTextObject(m_textAssets[index]);
            if (m_index == index) return;
            m_index = index;
            OnValueChanged?.Invoke(index);
        }

        public void Advance()
        {
            SetIndex(m_index + 1);
        }
    }
}
