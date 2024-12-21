// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Stores a reference to a physical hand to be found with a GetComponent
    /// </summary>
    public class PhysicalHandRef : MonoBehaviour
    {
        [SerializeField] private PhysicalHand hand;
        public PhysicalHand Hand { get => hand; private set { } }
    }
}
