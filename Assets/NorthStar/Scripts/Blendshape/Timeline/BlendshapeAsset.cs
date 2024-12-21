// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Playables;

namespace NorthStar
{
    public class BlendshapeAsset : PlayableAsset
    {
        public BlendshapeBehaviour Template;

        public int Index = 0;
        [Range(0, 100)]
        public float Intensity = 100.0f;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<BlendshapeBehaviour>.Create(graph, Template);

            var blendshapeBehaviour = playable.GetBehaviour();
            blendshapeBehaviour.Index = Index;
            blendshapeBehaviour.Intensity = Intensity;

            return playable;
        }
    }
}
