// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// This components modulates the volume of a sound based on rotation speed (optionally) around each axis
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class RotationSounds : MonoBehaviour
    {
        [SerializeField, AutoSet] private Rigidbody m_body;
        [SerializeField, AutoSet] private AudioSource m_audioSource;
        [SerializeField] private AnimationCurve m_curve;
        [SerializeField] private bool m_checkXAxis = true;
        [SerializeField] private bool m_checkYAxis = true;
        [SerializeField] private bool m_checkZAxis = true;
        public float Vector;

        private void OnValidate()
        {
            m_audioSource.playOnAwake = true;
            m_audioSource.loop = true;
        }

        private void Update()
        {
            var vector = m_body.angularVelocity;
            if (!m_checkXAxis) vector.x = 0;
            if (!m_checkYAxis) vector.y = 0;
            if (!m_checkZAxis) vector.z = 0;
            var speed = vector.magnitude;
            Vector = speed;
            m_audioSource.volume = m_curve.Evaluate(speed);
        }
    }
}
