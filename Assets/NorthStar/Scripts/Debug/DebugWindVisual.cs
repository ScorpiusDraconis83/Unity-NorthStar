// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Class to check the wind direction in game
    /// </summary>
    public class DebugWindVisual : MonoBehaviour
    {
        private void Update()
        {
            transform.position = BoatController.Instance.MovementSource.CurrentPosition + Vector3.up * 50;
            transform.forward = EnvironmentSystem.Instance.WindVector;
        }
    }
}
