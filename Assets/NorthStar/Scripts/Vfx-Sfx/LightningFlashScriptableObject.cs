// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using Meta.Utilities.Environment;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Data for a single lightning flash used by the storm controller
    /// </summary>
    [CreateAssetMenu(fileName = "LightningFlash", menuName = "ScriptableObjects/LightningFlashScriptableObject", order = 1)]
    public class LightningFlashScriptableObject : ScriptableObject
    {
        public EnvironmentProfile StrikeEnvironmentProfile;
        public EnvironmentProfile PostStrikeEnvironmentProfile;
        public float StrikeWarmupTime = 0.01f;
        public float StrikeCooldownTime = 0.65f;
        public float StrikeDuration = 1f;
        public float StrikeDistance = 25f;
        public float StrikeDirection = 90f;

        [Header("If any of these reflection areas are left empty they will not be updated for this lightning strike")]
        [SerializeField] private ReflectionTextures[] m_reflectionTextures;
        public AudioClip[] StrikeAudioClips;
        private Dictionary<ReflectionLocation, ReflectionTextures> m_reflectionLocations = new();

        public void Setup()
        {
            foreach (var tex in m_reflectionTextures)
            {
                if (m_reflectionLocations.ContainsKey(tex.ReflectionLocation))
                {
                    continue;
                }
                m_reflectionLocations.Add(tex.ReflectionLocation, tex);
            }
        }

        public ReflectionTextures GetTextures(ReflectionLocation location)
        {
            if (m_reflectionLocations.TryGetValue(location, out var value))
            {
                if (value.NoFlashTexture is not null && value.FlashTexture is not null)
                {
                    return value;
                }
            }
            return null;
        }

        [System.Serializable]
        public class ReflectionTextures
        {
            public ReflectionLocation ReflectionLocation;
            public Cubemap NoFlashTexture;
            public Cubemap FlashTexture;
        }
    }
}
