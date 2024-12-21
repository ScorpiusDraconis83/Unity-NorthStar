// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine.Playables;

namespace NorthStar
{
    /// <summary>
    /// Allows the ship's steering wheel to be controlled via the timeline
    /// </summary>
    public class WheelMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var binding = playerData as WheelController;

            var inputCount = playable.GetInputCount();

            var overrideValue = 0f;
            var doOverride = false;
            var bestWeight = 0f;
            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                var inputPlayable = (ScriptPlayable<WheelPlayableBehaviour>)playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();

                if (weight > bestWeight)
                {
                    doOverride = input.Override;
                    overrideValue = input.OverrideValue;
                    bestWeight = weight;
                }
            }

            binding.OverideValue = overrideValue;
            binding.UseOverride = doOverride;
        }
    }
}
