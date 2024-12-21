// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Playables;

namespace NorthStar
{
    public class WaveControlAsset : PlayableAsset
    {
        public WaveControlBehaviour Template;

        public float Distance;
        public float Height = 4;
        public float Width = 50;
        public float Length = 25;
        public float Steepness = 0.827f;
        public float CurveStrength = 0.75f;
        public float Angle;
        public Vector2 Center;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<WaveControlBehaviour>.Create(graph, Template);

            var waveControl = playable.GetBehaviour();

            waveControl.Distance = Distance;
            waveControl.Height = Height;
            waveControl.Width = Width;
            waveControl.Length = Length;
            waveControl.Steepness = Steepness;
            waveControl.CurveStrength = CurveStrength;
            waveControl.Angle = Angle;
            waveControl.Center = Center;

            return playable;
        }
    }
}
