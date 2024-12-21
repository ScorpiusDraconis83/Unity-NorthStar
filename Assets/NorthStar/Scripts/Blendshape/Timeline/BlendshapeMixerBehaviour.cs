// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine.Playables;

namespace NorthStar
{
    /// <summary>
    /// Playable behaviour that exposes control of NPC blendshapes via the timeline
    /// </summary>
    public class BlendshapeMixerBehaviour : PlayableBehaviour
    {
        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var trackBinding = playerData as BlendshapeManager;

            if (!trackBinding)
                return;

            var finalBlendShapeWeights = new float[trackBinding.BlendShapeCount];

            //Populate the blendshape array with -1 to signal that if it doesn't get assigned, it is not being used;
            for (var i = 0; i < finalBlendShapeWeights.Length; i++)
            {
                finalBlendShapeWeights[i] = -1;
            }

            var inputCount = playable.GetInputCount(); //get the number of all clips on this track

            for (var i = 0; i < inputCount; i++)
            {
                var inputWeight = playable.GetInputWeight(i);
                var inputPlayable = (ScriptPlayable<BlendshapeBehaviour>)playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();

                //Ensure the input index is within bounds of blendshape array
                if (input.Index < 0 || input.Index >= trackBinding.BlendShapeCount)
                {
                    continue;
                }

                // Use the above variables to process each frame of this playable.
                var strength = input.Intensity * inputWeight;

                //If any clip in the timeline uses this weight, even at strength 0, assume control over it
                if (finalBlendShapeWeights[input.Index] == -1)
                {
                    finalBlendShapeWeights[input.Index] = 0;
                }

                finalBlendShapeWeights[input.Index] += strength;
            }

            //Weights are applied last to avoid conflicting with multiple clips on the same timeline
            for (var i = 0; i < finalBlendShapeWeights.Length; i++)
            {
                if (finalBlendShapeWeights[i] != -1)
                {
                    trackBinding.BlendShapeWeights[i] = finalBlendShapeWeights[i];
                }
            }
        }
    }
}
