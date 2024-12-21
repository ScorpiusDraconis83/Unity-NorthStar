// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Playables;

namespace NorthStar
{
    public class BoatMovementAsset : PlayableAsset
    {
        public BoatMovementBehaviour Template;

        public BoatController.ReactionMovementData Movement = default;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<BoatMovementBehaviour>.Create(graph, Template);
            var boatMovement = playable.GetBehaviour();
            boatMovement.Movement = Movement;

            return playable;
        }
    }
}
