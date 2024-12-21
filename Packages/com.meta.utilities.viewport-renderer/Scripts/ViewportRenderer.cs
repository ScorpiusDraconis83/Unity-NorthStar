// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace NorthStar
{
    /// <summary>
    /// Renders the forward passes to a smaller viewport covering both eyes and with a tilted and adjusted projection matrix for use with telescope rendering
    /// </summary>
    [ExecuteAlways]
    public class ViewportRenderer : MonoBehaviour
    {
        public const int StencilBit = 1;

        private bool prewarm = false;
        private bool isRegistered = false;

        public class ViewportRenderPass : DrawObjectsPass
        {
            private static readonly int s_fogParams = Shader.PropertyToID("unity_FogParams");

            public ViewportRenderer ViewportRenderer;

            public ViewportRenderPass(string profilerTag, ShaderTagId[] shaderTagIds, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference) : base(profilerTag, shaderTagIds, opaque, evt, renderQueueRange, layerMask, stencilState, stencilReference)
            {
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cmd = renderingData.commandBuffer;
                if (cmd == null) return;

                try
                {
                    ref var cameraData = ref renderingData.cameraData;
                    var mainCamera = cameraData.camera;
                    var lensCamera = ViewportRenderer.lensCamera;
                    var scaleFactor = new Vector2(ScalableBufferManager.widthScaleFactor, ScalableBufferManager.heightScaleFactor);

                    // Cannot render without a lens camera
                    if (lensCamera == null) return;

                    // A lens mesh is required for sizing and stenciling
                    if (ViewportRenderer.lensMesh == null) return;

                    // Lens mesh handles writing stencil bits
                    cmd.DrawRenderer(ViewportRenderer.lensMesh, ViewportRenderer.lensMesh.sharedMaterial);

                    // Determine the pixel rect for each eye
                    var mesh = ViewportRenderer.lensMesh.GetComponent<MeshFilter>().sharedMesh;
                    var meshTransform = ViewportRenderer.lensMesh.transform.localToWorldMatrix;
                    Vector2 min = Vector2.positiveInfinity;
                    Vector2 max = Vector2.negativeInfinity;
                    Span<Rect> eyeRects = stackalloc Rect[2];
                    var cameraTarget = cameraData.cameraTargetDescriptor;
                    var screenSize = new Vector3(cameraTarget.width * scaleFactor.x, cameraTarget.height * scaleFactor.y);
                    for (int i = 0; i < 2; i++)
                    {
                        var eyeRect = GetBoundsRect(meshTransform, mesh.bounds, cameraData.GetProjectionMatrix(i) * cameraData.GetViewMatrix(i));
                        if (eyeRect.xMin >= 1f || eyeRect.yMin >= 1f || eyeRect.xMax <= -1f || eyeRect.yMax <= -1f) continue;
                        if (eyeRect.width <= 0f || eyeRect.height <= 0f) continue;
                        eyeRect.position = Vector2.Scale(eyeRect.position + Vector2.one, screenSize * 0.5f);
                        eyeRect.size = Vector2.Scale(eyeRect.size, screenSize * 0.5f);
                        eyeRects[i] = eyeRect;
                        min = Vector2.Min(min, eyeRect.min);
                        max = Vector2.Max(max, eyeRect.max);
                    }

                    // In single-pass mode, the viewport must cover both eyes
                    min = Vector2.Max(cameraData.pixelRect.min, min);
                    max = Vector2.Min(cameraData.pixelRect.max, max);
                    var clippedRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

                    // Dont render if nothing is visible
                    if (clippedRect.width <= 1f || clippedRect.height <= 1f)
                    {
                        if (ViewportRenderer.prewarm)
                        {
                            // Force render so that shaders are loaded
                            clippedRect = new Rect(0, 0, 1, 1);
                        }
                        else
                        {
                            return;
                        }
                    }

                    // Adjust fog density
                    var fogParams = Shader.GetGlobalVector(s_fogParams);
                    cmd.SetGlobalVector(s_fogParams, new(
                        fogParams.x * ViewportRenderer.fogDensityFactor,
                        fogParams.y * ViewportRenderer.fogDensityFactor,
                        fogParams.z, fogParams.w));
                    cmd.SetViewport(clippedRect);

                    // Get camera matrices
                    lensCamera.aspect = 1f;
                    var projection = lensCamera.projectionMatrix;
                    var view = mainCamera.worldToCameraMatrix;
                    var viewPos = view.MultiplyVector(-lensCamera.transform.position);
                    view.m03 = viewPos.x; view.m13 = viewPos.y; view.m23 = viewPos.z;

                    // Camera matrices need to be adjusted to clip rect
                    Matrix4x4 AdjustProjection(Matrix4x4 projection, Rect eyeRect)
                    {
                        var localFacing = mainCamera.transform.InverseTransformDirection(ViewportRenderer.transform.forward);
                        localFacing /= localFacing.z;
                        projection.m02 += 2f * (clippedRect.center.x - eyeRect.center.x) / clippedRect.width;
                        projection.m12 += 2f * (clippedRect.center.y - eyeRect.center.y) / clippedRect.height;
                        projection.m00 *= eyeRect.height / clippedRect.width;
                        projection.m11 *= eyeRect.height / clippedRect.height;
                        projection.m02 += projection.m00 * localFacing.x * ViewportRenderer.trackingFactor;
                        projection.m12 += projection.m11 * localFacing.y * ViewportRenderer.trackingFactor;
                        return projection;
                    }

                    // Apply camera matrices
                    if (cameraData.xr.enabled && cameraData.xr.singlePassEnabled)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            var eyeProj = AdjustProjection(projection, eyeRects[i]);
                            XRBuiltinShaderConstants.UpdateBuiltinShaderConstants(view, eyeProj, true, i, false, Matrix4x4.identity);
                        }
                        XRBuiltinShaderConstants.SetBuiltinShaderConstants(cmd);
                    }
                    else
                    {
                        var eyeProj = AdjustProjection(projection, eyeRects[0]);
                        cmd.SetViewProjectionMatrices(view, eyeProj);
                    }

                    // Render objects
                    base.Execute(context, ref renderingData);

                    // This needs to be stenciled to render only within the telescope lens
                    // otherwise, using fog colour should get us a reasonable result
                    //context.DrawSkybox(lensCamera);

                    // Restore camera data
                    cmd.SetGlobalVector(s_fogParams, fogParams);
                    cmd.SetViewport(new(0f, 0f, screenSize.x, screenSize.y));

                    if (cameraData.xr.enabled && cameraData.xr.singlePassEnabled)
                    {
                        XRBuiltinShaderConstants.Update(cameraData.xr, cmd, true);
                    }
                    else
                    {
                        cmd.SetViewProjectionMatrices(mainCamera.worldToCameraMatrix, mainCamera.projectionMatrix);
                    }

                    // Draw the shiny overlay mesh
                    if (ViewportRenderer.lensOverlay != null)
                    {
                        cmd.DrawRenderer(ViewportRenderer.lensOverlay, ViewportRenderer.lensOverlay.sharedMaterial);
                    }

                    if (ViewportRenderer.prewarm)
                    {
                        ViewportRenderer.CancelPrewarm();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            private Rect GetBoundsRect(Matrix4x4 worldMatrix, Bounds bounds, Matrix4x4 viewProj)
            {
                Vector3 min = Vector3.positiveInfinity;
                Vector3 max = Vector3.negativeInfinity;
                for (int i = 0; i < 8; i++)
                {
                    var corner = bounds.min + Vector3.Scale(bounds.size, new Vector3(i & 1, (i >> 1) & 1, (i >> 2) & 1));
                    corner = worldMatrix.MultiplyPoint3x4(corner);
                    var clipPos = viewProj * new Vector4(corner.x, corner.y, corner.z, 1f);
                    var spos = (Vector3)clipPos / Mathf.Abs(clipPos.w);
                    min = Vector3.Min(min, spos);
                    max = Vector3.Max(max, spos);
                }
                if (max.z < 0f) return default;
                return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            }
        }

        [SerializeField, Tooltip("Camera used for telescope viewport properties")]
        private Camera lensCamera;
        [SerializeField, Tooltip("Rendered before the telescope viewport to setup stenciling")]
        private MeshRenderer lensMesh;
        [SerializeField, Tooltip("Rendered after the telescope viewport as an overlay")]
        private MeshRenderer lensOverlay;
        [SerializeField, Tooltip("Multiplied into the fog density when rendering the telescope viewport")]
        private float fogDensityFactor = 1f;
        [SerializeField, Tooltip("How much the telescope should be fixed to its forward direction")]
        private float trackingFactor = 0.75f;

        private ViewportRenderPass renderPass;

        protected void OnEnable()
        {
            UpdateRegistered();
        }
        protected void OnDisable()
        {
            UpdateRegistered();
        }

        public void MarkForPrewarm()
        {
            prewarm = true;
            UpdateRegistered();
        }
        public void CancelPrewarm()
        {
            prewarm = false;
            UpdateRegistered();
        }

        private void UpdateRegistered()
        {
            var register = enabled || prewarm;
            if (isRegistered == register) return;
            isRegistered = register;

            if (isRegistered)
            {
                RenderPipelineManager.beginCameraRendering += OnBeginCamera;

                var stencilState = StencilState.defaultValue;
                stencilState.SetCompareFunction(CompareFunction.Equal);
                stencilState.SetPassOperation(StencilOp.Keep);
                stencilState.readMask = 1;
                var tags = new ShaderTagId[] { new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("UniversalForward"), new ShaderTagId("UniversalForwardOnly") };
                renderPass = new("Viewport Renderer", tags, false, RenderPassEvent.AfterRenderingTransparents,
                    RenderQueueRange.all, lensCamera.cullingMask, stencilState, StencilBit)
                {
                    ViewportRenderer = this,
                };
            }
            else
            {
                RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            }
        }

        private void OnBeginCamera(ScriptableRenderContext context, Camera cam)
        {
            cam.GetUniversalAdditionalCameraData()
                .scriptableRenderer.EnqueuePass(renderPass);
        }

    }
}
