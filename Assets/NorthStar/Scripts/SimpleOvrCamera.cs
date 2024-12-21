// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using Node = UnityEngine.XR.XRNode;

namespace NorthStar
{
    /// <summary>
    /// Bare-bones vr camera rig for testing
    /// </summary>
    public class SimpleOvrCamera : MonoBehaviour
    {
        public bool useAsw = true;
        public float framerate = 90;
        public bool dynamicFoveatedRendering = false;
        public OVRPlugin.FoveatedRenderingLevel FoveatedRenderingLevel = OVRPlugin.FoveatedRenderingLevel.Off;
        public bool updatePosition = true;

        private void Awake()
        {
            GetComponent<Camera>().depthTextureMode = DepthTextureMode.MotionVectors;

            OVRPlugin.systemDisplayFrequency = framerate;
            OVRManager.SetSpaceWarp(useAsw);
            OVRPlugin.useDynamicFoveatedRendering = dynamicFoveatedRendering;
            OVRPlugin.foveatedRenderingLevel = FoveatedRenderingLevel;
        }

        private void Update()
        {
            if (!updatePosition)
                return;

            if (OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.CenterEye, NodeStatePropertyType.Position, OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out var centerEyePosition))
                transform.localPosition = centerEyePosition;

            if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(Node.CenterEye, NodeStatePropertyType.Orientation, OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out var centerEyeRotation))
                transform.localRotation = centerEyeRotation;
        }
    }
}
