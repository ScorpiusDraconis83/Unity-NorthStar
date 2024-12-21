// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.Utilities;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Debug script used to enable/disable player renderers on demand
    /// </summary>
    public class PlayerRenderController : Singleton<PlayerRenderController>
    {
        [SerializeField, AutoSetFromChildren(IncludeInactive = true)] private Renderer[] m_renderers;

        static PlayerRenderController() => ProfilingSystem.AddBooleanCommand("player_render_active", SetPlayerRenderActive);

        private static void SetPlayerRenderActive(bool active)
        {
            if (Instance != null)
            {
                foreach (var renderer in Instance.m_renderers)
                {
                    renderer.enabled = active;
                }
            }
            Debug.Log($"player_render_active = {active}");
        }

        [ContextMenu("Set Active")]
        private void DebugSetActive() => SetPlayerRenderActive(true);

        [ContextMenu("Set Inactive")]
        private void DebugSetInactive() => SetPlayerRenderActive(false);
    }
}
