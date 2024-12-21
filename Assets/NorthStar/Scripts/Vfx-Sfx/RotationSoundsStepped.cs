// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// This component playes sounds periodically in response to stepped rotation around an axis (ratcheting behaviour)
    /// </summary>
    public class RotationSoundsStepped : MonoBehaviour
    {
        [SerializeField] private SoundPlayer m_player;
        [SerializeField] private Axis m_axis;
        [SerializeField] private float m_angle = 5;
        private Vector3 m_lastDirection;

        private enum Axis
        {
            Forward, Up, Right
        }

        private void Start()
        {
            var dir = m_axis switch
            {
                Axis.Up => Vector3.up,
                Axis.Right => Vector3.right,
                _ => Vector3.forward,
            };

            m_lastDirection = transform.localRotation * dir;
        }

        private void Update()
        {
            var dir = m_axis switch
            {
                Axis.Up => Vector3.up,
                Axis.Right => Vector3.right,
                _ => Vector3.forward,
            };

            dir = transform.localRotation * dir;

            if (Vector3.Angle(m_lastDirection, dir) > m_angle)
            {
                m_lastDirection = dir;
                m_player.Play();
            }
        }
    }
}
