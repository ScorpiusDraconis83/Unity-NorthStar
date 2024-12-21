// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Playables;

namespace NorthStar
{
    /// <summary>
    /// This playable behaviour mixes multiple boat reaction movements together on the timeline, allowing for complex movements to be created and edited via timelines
    /// </summary>
    public class BoatMovementMixerBehaviour : PlayableBehaviour
    {
        public int Slot;

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            var controller = info.output.GetUserData() as BoatController;
            if (controller == null) return;

            if (Application.isPlaying)
            {
                controller.ReactionMovement[Slot] = null;
            }
            else
            {
                controller.ResetToOriginalTransform();
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var controller = playerData as BoatController;
            if (controller is null) return;

            var inputCount = playable.GetInputCount();

            var finalMovement = new BoatController.TransformOffset()
            {
                Position = Vector3.zero,
                Rotation = Quaternion.identity
            };

            for (var i = 0; i < inputCount; i++)
            {
                var inputWeight = playable.GetInputWeight(i);
                var inputPlayable = (ScriptPlayable<BoatMovementBehaviour>)playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();

                var time = (float)inputPlayable.GetTime();
                var duration = (float)inputPlayable.GetDuration();


                var t = time / duration;
                var ease = (input.Movement.EaseCurve?.Evaluate(t) ?? 1) * inputWeight;
                var position = input.Movement.PositionOffset * ease;
                var rotation = Quaternion.Euler(input.Movement.AngularOffset * ease);

                finalMovement.Position += position;
                finalMovement.Rotation *= rotation;
            }

            if (!Application.isPlaying)
            {
                controller.PreviewReactionMovementInEditor(finalMovement, Slot);
            }
            else
            {
                controller.ReactionMovement[Slot] = finalMovement;
            }

        }
    }
}
