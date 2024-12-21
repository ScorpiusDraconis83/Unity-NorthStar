// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Plays a vfx and disabes an object after a time
    /// </summary>
    public class CrateDropSequence : MonoBehaviour
    {
        [SerializeField] private float m_despawnDelay;
        [SerializeField] private EffectAsset m_effectAsset;
        private bool m_triggered;
        private float m_time;
        public void Trigger()
        {
            m_triggered = true;
            m_time = 0;
        }

        private void Update()
        {
            if (!m_triggered)
                return;
            m_time += Time.deltaTime;
            if (m_time > m_despawnDelay)
            {
                gameObject.SetActive(false);
                m_effectAsset.Play(transform.position, transform.rotation, true);
            }
        }
    }
}
