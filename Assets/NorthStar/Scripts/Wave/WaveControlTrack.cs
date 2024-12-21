// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine.Timeline;

namespace NorthStar
{
    [TrackClipType(typeof(WaveControlAsset))]
    [TrackBindingType(typeof(BoatController))]
    public class WaveControlTrack : TrackAsset
    {
    }
}
