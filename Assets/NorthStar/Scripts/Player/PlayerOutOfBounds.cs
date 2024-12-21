// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Detects when the player is out of bounds to draw a special visual indicator to guide the player back
    /// </summary>
    public class PlayerOutOfBounds : MonoBehaviour
    {
        [SerializeField] private float m_headCheckRadius;
        [SerializeField] private LayerMask m_layerMask;

        [SerializeField] private float m_fadeTime = 2f;
        [SerializeField] private ScreenFader m_fader;
        [SerializeField] private GrabTeleport m_grabTeleport;

        private float m_timer;

        private static bool s_enabled = true;

        static PlayerOutOfBounds()
        {
            ProfilingSystem.AddBooleanCommand("out_of_bounds_fade_enabled", enabled =>
            {
                s_enabled = enabled;
                Debug.Log($"out_of_bounds_fade_enabled = {s_enabled}");
            });
        }

        [ContextMenu("Toggle Debug Enabled")]
        private void ToggleDebugEnabled()
        {
            s_enabled = !s_enabled;
            Debug.Log($"out_of_bounds_fade_enabled = {s_enabled}");
        }

        private void Update()
        {
            m_timer = Mathf.Clamp(m_timer + Time.deltaTime * (HeadOutOfBounds() ? 1 : -1), 0f, m_fadeTime);
            m_fader.HeadFadeValue = EasingFunc(m_timer / m_fadeTime);
        }

        private bool HeadOutOfBounds()
        {
            return s_enabled && (Physics.CheckSphere(transform.position, m_headCheckRadius, m_layerMask) || HorizontalDistance(transform.position, m_grabTeleport.LastTeleport.transform.position) > m_grabTeleport.LastTeleport.PlayerBoundsRadius);
        }

        private float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0;
            b.y = 0;
            return Vector3.Distance(a, b);
        }

        private float EasingFunc(float t)
        {
            return Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
        }
    }
}
