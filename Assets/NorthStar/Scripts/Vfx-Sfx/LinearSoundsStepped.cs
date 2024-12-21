// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// This component playes sounds periodically in response to stepped linear motion in a given axis
    /// </summary>
    public class LinearSoundsStepped : MonoBehaviour
    {
        private enum Axis
        {
            x, y, z
        }

        [SerializeField, AutoSet] private SoundPlayer m_sound;
        [SerializeField] private Axis m_axis;
        [SerializeField] private float m_triggerDistance = .1f;

        private float m_lastPos;

        private void Start()
        {
            m_lastPos = GetPos();
        }
        private void Update()
        {
            if (Mathf.Abs(m_lastPos - GetPos()) > m_triggerDistance)
            {
                m_lastPos = GetPos();
                m_sound.Play();
            }
        }

        private float GetPos()
        {
            return m_axis switch
            {
                Axis.x => transform.localPosition.x,
                Axis.y => transform.localPosition.y,
                Axis.z => transform.localPosition.z,
                _ => 0,
            };
        }
    }
}
