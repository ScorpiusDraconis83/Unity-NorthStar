// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace NorthStar
{
    [TrackClipType(typeof(BlendshapeAsset))]
    [TrackBindingType(typeof(BlendshapeManager))]
    public class BlendshapeTrack : TrackAsset
    {
        public BlendshapeBehaviour Template;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            UpdateClipNames(go);

            return ScriptPlayable<BlendshapeMixerBehaviour>.Create(graph, inputCount);
        }

        private void UpdateClipNames(GameObject go)
        {
            var blendShapeManager = go.GetComponent<PlayableDirector>().GetGenericBinding(this) as BlendshapeManager;
            Mesh sharedMesh = null;
            if (blendShapeManager != null)
            {
                sharedMesh = blendShapeManager.SkinnedMeshRenderer.sharedMesh;
            }

            foreach (var clip in GetClips())
            {
                var asset = clip.asset as BlendshapeAsset;

                if (sharedMesh)
                {
                    //Ensure the input index is within bounds of blendshape array
                    if (asset.Index < 0 || asset.Index >= sharedMesh.blendShapeCount)
                    {
                        continue;
                    }

                    //Figure out what the new name would be incase we are already using that name
                    var desiredName = sharedMesh.GetBlendShapeName(asset.Index);
                    if (desiredName == clip.displayName)
                    {
                        continue;
                    }

                    var usingExistingBlendshapeName = false;
                    for (var i = 0; i < sharedMesh.blendShapeCount; i++)
                    {
                        if (clip.displayName == sharedMesh.GetBlendShapeName(i))
                        {
                            usingExistingBlendshapeName = true;
                        }
                    }

                    if (!usingExistingBlendshapeName && clip.displayName != "BlendshapeAsset")
                    {
                        continue;
                    }

                    clip.displayName = desiredName;
                }
            }
        }
    }
}
