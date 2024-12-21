// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace NorthStar
{
    [TrackClipType(typeof(LookAtAsset))]
    [TrackBindingType(typeof(NpcRigController))]
    public class LookAtTrack : TrackAsset
    {
        public LookAtBehaviour Template;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            UpdateClipNames(go);

            return ScriptPlayable<LookAtMixerBehaviour>.Create(graph, inputCount);
        }

        private void UpdateClipNames(GameObject go)
        {
            var npcRigController = go.GetComponent<PlayableDirector>().GetGenericBinding(this) as NpcRigController;

            var desiredName = " Look-at ";
            if (npcRigController != null)
            {
                desiredName = npcRigController.gameObject.name + desiredName;
            }


            foreach (var clip in GetClips())
            {
                clip.displayName = desiredName + ((LookAtAsset)clip.asset).Rig;
            }
        }
    }
}
