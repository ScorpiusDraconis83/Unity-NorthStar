// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class ClothWind : MonoBehaviour
    {
        public Cloth[] clothObjects;
        public float baseStrength = 5;
        public float turbulenceAmount = 0;
        public float windFactor = 0;//modify base wind by value 0-1

        private void FixedUpdate()
        {
            foreach (var c in clothObjects)
            {
                c.randomAcceleration = transform.right.normalized * -turbulenceAmount;
                c.externalAcceleration = transform.right.normalized * -(baseStrength * windFactor);
            }
        }

    }
}