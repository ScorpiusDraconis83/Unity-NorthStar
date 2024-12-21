// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace NorthStar
{
    [TrackClipType(typeof(BoatMovementAsset))]
    [TrackBindingType(typeof(BoatController))]

    public class BoatMovementTrack : TrackAsset
    {
        public int Slot;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<BoatMovementMixerBehaviour>.Create(graph, inputCount);

            mixer.GetBehaviour().Slot = Slot;

            return mixer;
        }
    }
}
