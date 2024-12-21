// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Playables;

namespace NorthStar
{
    public class LookAtAsset : PlayableAsset
    {
        public LookAtBehaviour Template;

        [Range(0, 1)]
        public float Weight = 1;
        public NpcRigController.IKRig Rig = NpcRigController.IKRig.Spine;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<LookAtBehaviour>.Create(graph, Template);

            var lookAtBehaviour = playable.GetBehaviour();
            lookAtBehaviour.Weight = Weight;
            lookAtBehaviour.Rig = Rig;

            return playable;
        }
    }
}
