// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Events;

namespace NorthStar
{
    /// <summary>
    /// Triggers a unity event when count reaches a defined value
    /// </summary>
    public class Counter : MonoBehaviour
    {
        public int Count { get; private set; }
        [field: SerializeField] public int MaxCount { get; private set; } = 1;
        [SerializeField] public UnityEvent OnComplete;

        [SerializeField] private bool m_decay;
        [SerializeField] private float m_decayTime;
        [SerializeField] private bool m_smoothDecay;
        [SerializeField] private bool m_disableOnComplete;

        private float m_timeLastIncremented;

        private void Update()
        {
            if (!m_decay)
                return;
            if (Time.time - m_timeLastIncremented > m_decayTime)
                Count = m_smoothDecay ? Mathf.Max(0, Count - 1) : 0;
        }

        public void Increment()
        {
            if (!enabled)
                return;
            Count++;
            if (Count >= MaxCount)
            {
                OnComplete.Invoke();
                if (m_disableOnComplete)
                    enabled = false;
            }
            m_timeLastIncremented = Time.time;
        }
    }
}
