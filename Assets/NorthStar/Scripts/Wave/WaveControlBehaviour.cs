// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities.Environment;
using UnityEngine;
using UnityEngine.Playables;

namespace NorthStar
{
    /// <summary>
    /// This playable behaviour controls explict ocean wave VFX, allowing them to be precisely edited and controlled via timelines
    /// </summary>
    [System.Serializable]
    public class WaveControlBehaviour : PlayableBehaviour
    {
        private static readonly int s_giantWaveDelta = Shader.PropertyToID("_GiantWaveDelta");
        private static readonly int s_giantWaveDuration = Shader.PropertyToID("_GiantWaveDuration");
        private static readonly int s_giantWaveDistance = Shader.PropertyToID("_GiantWaveDistance");
        private static readonly int s_giantWaveCenter = Shader.PropertyToID("_Center");
        private static readonly int s_giantWaveHeight = Shader.PropertyToID("_GiantWaveHeight");
        private static readonly int s_giantWaveWidth = Shader.PropertyToID("_GiantWaveWidth");
        private static readonly int s_giantWaveLength = Shader.PropertyToID("_GiantWaveLength");
        private static readonly int s_giantWaveSteepness = Shader.PropertyToID("_GiantWaveSteepness");
        private static readonly int s_giantWaveCurveStrength = Shader.PropertyToID("_GiantWaveCurveStrength");
        private static readonly int s_giantWaveAngle = Shader.PropertyToID("_GiantWaveAngle");

        public float Distance;
        public float Height;
        public float Width;
        public float Length;
        public float Steepness;
        public float CurveStrength;
        public float Angle;
        public Vector2 Center;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            var oceanMaterial = EnvironmentSystem.Instance.CurrentProfile.OceanMaterial;
            oceanMaterial.EnableKeyword("_GIANT_WAVE_ENABLED");
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            var oceanMaterial = EnvironmentSystem.Instance.CurrentProfile.OceanMaterial;
            oceanMaterial.DisableKeyword("_GIANT_WAVE_ENABLED");
            Shader.SetGlobalFloat(s_giantWaveDelta, 0);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var time = (float)playable.GetTime();
            var duration = (float)playable.GetDuration();

            var boatController = (BoatController)playerData;

            var center = Quaternion.AngleAxis(boatController.HeadingAngle, Vector3.up) * new Vector3(Center.x, 0, Center.y);

            Shader.SetGlobalFloat(s_giantWaveDelta, duration - time);
            Shader.SetGlobalFloat(s_giantWaveDuration, duration);
            Shader.SetGlobalFloat(s_giantWaveDistance, Distance);

            var angle = Angle - boatController.HeadingAngle;


            var oceanMaterial = EnvironmentSystem.Instance.CurrentProfile.OceanMaterial;
            oceanMaterial.SetFloat(s_giantWaveWidth, Width);
            oceanMaterial.SetFloat(s_giantWaveHeight, Height);
            oceanMaterial.SetFloat(s_giantWaveLength, Length);
            oceanMaterial.SetFloat(s_giantWaveAngle, angle / 360);
            oceanMaterial.SetFloat(s_giantWaveSteepness, Steepness);
            oceanMaterial.SetFloat(s_giantWaveCurveStrength, CurveStrength);
            oceanMaterial.SetVector(s_giantWaveCenter, new Vector2(center.x, center.z));
        }
    }
}
