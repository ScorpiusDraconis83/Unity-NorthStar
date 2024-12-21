// Copyright (c) Meta Platforms, Inc. and affiliates.
using System;
using UnityEngine.Playables;

namespace NorthStar
{
    /// <summary>
    /// PlayableBehaviour that mixes tracks for look-at behaviour on NPCs, allowing it to be controlled via timelines
    /// </summary>
    public class LookAtMixerBehaviour : PlayableBehaviour
    {
        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var trackBinding = playerData as NpcRigController;

            if (!trackBinding)
                return;

            var finalWeights = new float[Enum.GetNames(typeof(NpcRigController.IKRig)).Length];
            //Populate the blendshape array with -1 to signal that if it doesn't get assigned, it is not being used;
            for (var i = 0; i < finalWeights.Length; i++)
            {
                finalWeights[i] = -1;
            }

            var inputCount = playable.GetInputCount(); //get the number of all clips on this track

            for (var i = 0; i < inputCount; i++)
            {
                var inputWeight = playable.GetInputWeight(i);
                var inputPlayable = (ScriptPlayable<LookAtBehaviour>)playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();

                //Ensure the input index is within bounds of blendshape array
                if ((int)input.Rig < 0 || (int)input.Rig >= finalWeights.Length)
                {
                    continue;
                }

                // Use the above variables to process each frame of this playable.
                var strength = input.Weight * inputWeight;

                //If any clip in the timeline uses this weight, even at strength 0, assume control over it
                if (finalWeights[(int)input.Rig] == -1)
                {
                    finalWeights[(int)input.Rig] = 0;
                }

                finalWeights[(int)input.Rig] += strength;
                //trackBinding.SetBlendShapeWeight(input.Index, input.Intensity * inputWeight);
            }

            //Weights are applied last to avoid conflicting with multiple clips on the same timeline
            for (var i = 0; i < finalWeights.Length; i++)
            {
                if (finalWeights[i] != -1)
                {
                    trackBinding.SetWeightCap(finalWeights[i], (NpcRigController.IKRig)i);
                }
            }
        }
    }
}
