// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using Oculus.Movement.AnimationRigging;
using Oculus.Movement.Utils;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Scales the player avatar model to match the player's height calibration
    /// </summary>
    public class PlayerCalibration : MonoBehaviour
    {
        [SerializeField] private Animator m_animator;
        [SerializeField] private Transform m_cameraRig;
        [SerializeField] private float m_avatarHeight;
        [SerializeField] private RestPoseObjectHumanoid m_restPose;
        [SerializeField, AutoSet(typeof(BodyPositions))] private BodyPositions m_bodyPositions;

        private void Start()
        {
            OnPlayerCalibrationChange();
        }

        private void OnEnable()
        {
            GlobalSettings.PlayerSettings.OnPlayerCalibrationChange += OnPlayerCalibrationChange;
        }

        private void OnDisable()
        {
            GlobalSettings.PlayerSettings.OnPlayerCalibrationChange -= OnPlayerCalibrationChange;
        }

        private void OnPlayerCalibrationChange()
        {
            if (!ReferenceEquals(m_cameraRig, null))
            {
                var playerSettings = GlobalSettings.PlayerSettings;
                var yOffset = playerSettings.Seated ? (playerSettings.Height - playerSettings.SeatedHeight) / 100.0f : 0;
                m_cameraRig.localPosition = Vector3.up * yOffset;

                var heightRatio = playerSettings.Height / 100.0f / m_avatarHeight;
                m_animator.transform.localScale = Vector3.one * heightRatio;

                var constraint = GetComponentInChildren<FullBodyDeformationConstraint>();
                var constraintData = constraint.data;
                constraintData.InitializeStartingScale();
                constraintData.SetUpHipsAndHeadBones();
                constraintData.SetUpAdjustments(m_restPose);
                constraint.data = constraintData;

                var heightDiff = m_avatarHeight * heightRatio - m_avatarHeight;
                m_animator.transform.localPosition = new Vector3(0, -heightDiff, 0);

                m_bodyPositions.HandVisuals[0].localScale = Vector3.one * heightRatio;
                m_bodyPositions.HandVisuals[1].localScale = Vector3.one * heightRatio;
            }
        }
    }
}
