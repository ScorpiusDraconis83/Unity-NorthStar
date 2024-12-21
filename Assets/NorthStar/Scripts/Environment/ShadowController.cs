// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NorthStar
{
    /// <summary>
    /// Used to control some settings related to shadows and shadow importance volumes
    /// </summary>
    public class ShadowController : MonoBehaviour
    {
        public void SetShadowsEnabled(bool enabled)
        {
            var camera = Camera.main;
            if (camera != null && camera.TryGetComponent(out UniversalAdditionalCameraData universalCameraData))
            {
                universalCameraData.renderShadows = enabled;
            }
        }

        public void SetMaxShadowDistanceOverride(float distance)
        {
            ShadowImportanceVolume.DefaultCollection.MaximumDistanceOverride = distance;
        }

        public void ClearMaxShadowDistanceOverride()
        {
            SetMaxShadowDistanceOverride(float.MaxValue);
        }
    }
}