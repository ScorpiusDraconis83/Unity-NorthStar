// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace NorthStar
{
    public class JiggleWindSetter : MonoBehaviour
    {
        [Header("Test Script - Overwrites Jiggle Rig wind setting with a force over sine(time)")]
        [Space]
        public Vector3 WindVector = Vector3.zero;
        public float Magnitude = 1;
        public float Frequency = 1;

        private void Update()
        {
            var turbulance = Mathf.Sin(Time.time * Frequency) * Magnitude;
            var updatedWind = WindVector * turbulance;
            GetComponent<JigglePhysics.JiggleRigBuilder>().wind = updatedWind;
        }
    }
}
