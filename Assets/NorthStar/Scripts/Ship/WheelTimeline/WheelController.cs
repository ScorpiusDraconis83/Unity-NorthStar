// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Optionally overrides the steering value for the ship's wheel
    /// </summary>
    public class WheelController : MonoBehaviour
    {
        [SerializeField] private BaseJointInteractable<float> m_interactable;

        [Range(-1, 1)] public float OverideValue, MinValue = -1, MaxValue = 1;
        public bool UseOverride = false;

        public float Value => Mathf.Clamp(UseOverride ? OverideValue : m_interactable.Value, MinValue, MaxValue);
    }
}
