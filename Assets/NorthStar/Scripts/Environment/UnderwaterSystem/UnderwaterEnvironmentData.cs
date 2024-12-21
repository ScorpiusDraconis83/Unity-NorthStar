// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Stores variables for underwater rendering.
    /// </summary>
    [CreateAssetMenu(fileName = "UnderwaterData", menuName = "Environment System Data/Underwater Data")]
    public class UnderwaterEnvironmentData : ScriptableObject
    {
        [Header("Global Settings")]
        public bool useUnderwaterFog = true;

        [Header("Base Caustic Properties")]
        public float causticScale = 1f;
        public float causticSpeed = 1f;
        public float causticTimeModulation = 0.001f;
        public float causticEmissiveIntensity = 0.1f;

        [Header("Caustic Distortion")]
        public float distortionIntensity = 0.1f;
        public Vector2 distortionScale = new(1.0f, 1.0f);
        public Vector2 distortionSpeed = new(0.1f, 0.1f);

        private void OnValidate()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                var controllers = FindObjectsOfType<UnderwaterEnvironmentController>();
                foreach (var controller in controllers)
                {
                    if (controller.Parameters == this)
                    {
                        controller.UpdateCausticParameters();
                    }
                }
            };
#endif
        }
    }
}