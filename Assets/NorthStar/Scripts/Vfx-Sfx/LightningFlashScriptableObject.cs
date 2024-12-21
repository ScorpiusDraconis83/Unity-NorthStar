// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Data for a single lightning flash used by the storm controller
    /// </summary>
    [CreateAssetMenu(fileName = "LightningFlash", menuName = "ScriptableObjects/LightningFlashScriptableObject", order = 1)]
    public class LightningFlashScriptableObject : ScriptableObject
    {
        public EnvironmentProfile strikeEnvironmentProfile;
        public EnvironmentProfile postStrikeEnvironmentProfile;
        [Tooltip("It is recommended to keep this low to avoid visual glitches"), Range(0.01f, 1f)] public float strikeWarmupTime = 0.01f;
        [Range(0.01f, 2f)] public float strikeCooldownTime = 0.65f;
        [Tooltip("How long the strike will last at full strength in seconds"), Range(0.1f, 2f)] public float strikeDuration = 1f;
        [Tooltip("Distance that lightning bolt will strike from boat in Unity units")] public float strikeDistance = 25f;
        [Tooltip("In degrees, clockwise, from boat forward direction"), Range(0, 359)] public float strikeDirection = 90f;

        [Header("If any of these reflection areas are left empty they will not be updated for this lightning strike")]
        public ReflectionTextures[] m_reflectionTextures;
        public AudioClip[] strikeAudioClips;
        private Dictionary<ReflectionLocation, ReflectionTextures> m_reflectionLocations = new();

        public void Setup()
        {
            foreach (var tex in m_reflectionTextures)
            {
                if (m_reflectionLocations.ContainsKey(tex.reflectionLocation))
                {
                    continue;
                }
                m_reflectionLocations.Add(tex.reflectionLocation, tex);
            }
        }

        public ReflectionTextures GetTextures(ReflectionLocation location)
        {
            if (m_reflectionLocations.TryGetValue(location, out var value))
            {
                if (value.noFlashTexture is not null && value.flashTexture is not null)
                {
                    return value;
                }
            }
            return null;
        }

        [System.Serializable]
        public class ReflectionTextures
        {
            public ReflectionLocation reflectionLocation;
            public Cubemap noFlashTexture;
            public Cubemap flashTexture;
        }
    }
}
