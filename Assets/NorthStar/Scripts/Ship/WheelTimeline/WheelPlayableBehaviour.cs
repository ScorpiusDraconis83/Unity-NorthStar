// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine.Playables;

namespace NorthStar
{
    [System.Serializable]
    public class WheelPlayableBehaviour : PlayableBehaviour
    {
        public float OverrideValue = 0;
        public bool Override = false;
    }
}
