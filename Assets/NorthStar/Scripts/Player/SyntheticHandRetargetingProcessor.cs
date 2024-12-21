// Copyright (c) Meta Platforms, Inc. and affiliates.
using System;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Movement.AnimationRigging;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Performs some post-processing on elbow, wrist and finger joints to more closely match the player's hand tracking while also correcting twist bones and elbow orientation
    /// </summary>
    [CreateAssetMenu(fileName = "Synthetic Hand", menuName = "Data/Retargeting Processors/Synthetic Hand",
        order = 1)]
    public class SyntheticHandRetargetingProcessor : RetargetingProcessor
    {
        public struct FingerBoneMapping
        {
            public HumanBodyBones HumanBone;
            public HandJointId HandJoint;
        }

        public class HandMappingData
        {
            public List<FingerBoneMapping> Mapping;
        }

        private readonly Tuple<HumanBodyBones, HandJointId>[] m_leftHandBonePairs =
        {
            new (HumanBodyBones.LeftThumbProximal, HandJointId.HandThumb1),
            new (HumanBodyBones.LeftThumbIntermediate, HandJointId.HandThumb2),
            new (HumanBodyBones.LeftThumbDistal, HandJointId.HandThumb3),
            new (HumanBodyBones.LeftIndexProximal, HandJointId.HandIndex1),
            new (HumanBodyBones.LeftIndexIntermediate, HandJointId.HandIndex2),
            new (HumanBodyBones.LeftIndexDistal, HandJointId.HandIndex3),
            new (HumanBodyBones.LeftMiddleProximal, HandJointId.HandMiddle1),
            new (HumanBodyBones.LeftMiddleIntermediate, HandJointId.HandMiddle2),
            new (HumanBodyBones.LeftMiddleDistal, HandJointId.HandMiddle3),
            new (HumanBodyBones.LeftRingProximal, HandJointId.HandRing1),
            new (HumanBodyBones.LeftRingIntermediate, HandJointId.HandRing2),
            new (HumanBodyBones.LeftRingDistal, HandJointId.HandRing3),
            new (HumanBodyBones.LeftLittleProximal, HandJointId.HandPinky1),
            new (HumanBodyBones.LeftLittleIntermediate, HandJointId.HandPinky2),
            new (HumanBodyBones.LeftLittleDistal, HandJointId.HandPinky3),
        };

        private readonly Tuple<HumanBodyBones, HandJointId>[] m_rightHandBonePairs =
{
            new (HumanBodyBones.RightThumbProximal, HandJointId.HandThumb1),
            new (HumanBodyBones.RightThumbIntermediate, HandJointId.HandThumb2),
            new (HumanBodyBones.RightThumbDistal, HandJointId.HandThumb3),
            new (HumanBodyBones.RightIndexProximal, HandJointId.HandIndex1),
            new (HumanBodyBones.RightIndexIntermediate, HandJointId.HandIndex2),
            new (HumanBodyBones.RightIndexDistal, HandJointId.HandIndex3),
            new (HumanBodyBones.RightMiddleProximal, HandJointId.HandMiddle1),
            new (HumanBodyBones.RightMiddleIntermediate, HandJointId.HandMiddle2),
            new (HumanBodyBones.RightMiddleDistal, HandJointId.HandMiddle3),
            new (HumanBodyBones.RightRingProximal, HandJointId.HandRing1),
            new (HumanBodyBones.RightRingIntermediate, HandJointId.HandRing2),
            new (HumanBodyBones.RightRingDistal, HandJointId.HandRing3),
            new (HumanBodyBones.RightLittleProximal, HandJointId.HandPinky1),
            new (HumanBodyBones.RightLittleIntermediate, HandJointId.HandPinky2),
            new (HumanBodyBones.RightLittleDistal, HandJointId.HandPinky3),
        };

        [SerializeField] private HandMappingData m_leftHandData;
        [SerializeField] private HandMappingData m_rightHandData;

        [NonSerialized] public HandVisual LeftHandVisual;
        [NonSerialized] public HandVisual RightHandVisual;

        private void RetargetHand(RetargetingLayer retargetingLayer, HandVisual hand, Tuple<HumanBodyBones, HandJointId>[] handBonePairs)
        {
            var left = hand == LeftHandVisual;

            var animator = retargetingLayer.GetAnimatorTargetSkeleton();
            var targetWrist = hand.GetTransformByHandJointId(HandJointId.HandWristRoot);
            var visualWrist = animator.GetBoneTransform(left ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);

            var arm = animator.GetBoneTransform(left ? HumanBodyBones.LeftUpperArm : HumanBodyBones.RightUpperArm);
            var elbow = animator.GetBoneTransform(left ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm);
            var elbowTwist = elbow.GetChild(0);

            // For some reason the hand retargeting has some drift, so here we correct the elbow, twist and wrist bones to their correct locations NO MATTER WHAT
            var visualWristOriginalRotation = visualWrist.rotation;

            var elbowCorrectionQuaternion = Quaternion.Euler(-90, left ? 90 : -90, 0);

            var elbowToWrist = (elbow.position - targetWrist.position).normalized;

            elbow.rotation = Quaternion.LookRotation(elbowToWrist, arm.forward) * elbowCorrectionQuaternion;
            elbowTwist.rotation = Quaternion.LookRotation(elbowToWrist, Vector3.Lerp(arm.forward, targetWrist.up, 0.5f)) * elbowCorrectionQuaternion;

            visualWrist.position = targetWrist.position;
            visualWrist.rotation = visualWristOriginalRotation;

            // Retarget finger bones to match synthetic hand visuals
            foreach (var pair in handBonePairs)
            {
                var correctionQuaternion = retargetingLayer.GetCorrectionQuaternion(pair.Item1);

                var pose = hand.GetJointPose(pair.Item2, Space.World);
                var t = animator.GetBoneTransform(pair.Item1);

                t.position = pose.position - targetWrist.position + visualWrist.position;
                t.rotation = pose.rotation * correctionQuaternion ?? Quaternion.identity;
            }
        }

        public override void ProcessRetargetingLayer(RetargetingLayer retargetingLayer, IList<OVRBone> ovrBones)
        {
            // Only retarget if a hand/controller is actually available/connected
            if (OVRInput.IsControllerConnected(OVRInput.Controller.LHand) || OVRInput.IsControllerConnected(OVRInput.Controller.LTouch))
                RetargetHand(retargetingLayer, LeftHandVisual, m_leftHandBonePairs);

            if (OVRInput.IsControllerConnected(OVRInput.Controller.RHand) || OVRInput.IsControllerConnected(OVRInput.Controller.RTouch))
                RetargetHand(retargetingLayer, RightHandVisual, m_rightHandBonePairs);
        }
    }
}
