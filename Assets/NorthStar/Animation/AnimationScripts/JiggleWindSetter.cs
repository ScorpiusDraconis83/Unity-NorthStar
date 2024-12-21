// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace NorthStar
{
    public class JiggleWindSetter : MonoBehaviour
    {
        [Header("Test Script - Overwrites Jiggle Rig wind setting with a force over sine(time)")]
        [Space]
        public Vector3 windVector = Vector3.zero;
        public float magnitude = 1;
        public float frequency = 1;

        private void Update()
        {
            var turbulance = Mathf.Sin(Time.time * frequency) * magnitude;
            var updatedWind = windVector * turbulance;
            GetComponent<JigglePhysics.JiggleRigBuilder>().wind = updatedWind;
        }
    }
}
