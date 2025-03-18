// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Legacy;
using static UnityEditor.Rendering.Universal.ShaderGraph.SubShaderUtils;
using static Unity.Rendering.Universal.ShaderUtils;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.Universal.ShaderGraph
{
    // This is based off of UniversalUnlitSubTarget with support for keywords required for
    // handling lighting in a custom way and some other configurable features.
    // The intention is allow shadow sampling, PBR lighting, and fog in a custom way
    sealed class UniversalCustomLitSubTarget : UniversalSubTarget
    {
        static readonly GUID kSourceCodeGuid = new GUID("9e75b81b0b0446945890d6834d48d288"); // UniversalCustomLitSubTarget.cs

        public enum CustomFeatureModes { None, Standard, Custom, }

        [SerializeField]
        bool m_EnableAdditionalLights = false;

        [SerializeField]
        bool m_EnableLightCookies = false;

        [SerializeField]
        bool m_ReceiveShadows = true;

        [SerializeField]
        CustomFeatureModes m_FogMode = CustomFeatureModes.Standard;

        public override int latestVersion => 2;

        public UniversalCustomLitSubTarget()
        {
            displayName = "Custom Lit";
        }

        protected override ShaderID shaderID => ShaderID.SG_Unlit;

        public override bool IsActive() => true;

        public override void Setup(ref TargetSetupContext context)
        {
            context.AddAssetDependency(kSourceCodeGuid, AssetCollection.Flags.SourceDependency);
            base.Setup(ref context);

            var universalRPType = typeof(UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset);
            if (!context.HasCustomEditorForRenderPipeline(universalRPType))
            {
                var gui = typeof(ShaderGraphUnlitGUI);
                context.AddCustomEditorForRenderPipeline(gui.FullName, universalRPType);
            }
            // Process SubShaders
            context.AddSubShader(PostProcessSubShader(SubShaders.CustomLit(target, target.renderType, target.renderQueue, target.disableBatching, m_EnableAdditionalLights, m_EnableLightCookies, m_FogMode)));
        }

        public override void ProcessPreviewMaterial(Material material)
        {
            if (target.allowMaterialOverride)
            {
                // copy our target's default settings into the material
                // (technically not necessary since we are always recreating the material from the shader each time,
                // which will pull over the defaults from the shader definition)
                // but if that ever changes, this will ensure the defaults are set
                material.SetFloat(Property.SurfaceType, (float)target.surfaceType);
                material.SetFloat(Property.BlendMode, (float)target.alphaMode);
                material.SetFloat(Property.AlphaClip, target.alphaClip ? 1.0f : 0.0f);
                material.SetFloat(Property.CullMode, (int)target.renderFace);
                material.SetFloat(Property.CastShadows, target.castShadows ? 1.0f : 0.0f);
                material.SetFloat(Property.ZWriteControl, (float)target.zWriteControl);
                material.SetFloat(Property.ZTest, (float)target.zTestMode);
            }

            // We always need these properties regardless of whether the material is allowed to override
            // Queue control & offset enable correct automatic render queue behavior
            // Control == 0 is automatic, 1 is user-specified render queue
            material.SetFloat(Property.QueueOffset, 0.0f);
            material.SetFloat(Property.QueueControl, (float)BaseShaderGUI.QueueControl.Auto);

            // call the full unlit material setup function
            ShaderGraphUnlitGUI.UpdateMaterial(material, MaterialUpdateType.CreatedNewMaterial);
        }

        public override void GetFields(ref TargetFieldContext context)
        {
            base.GetFields(ref context);
        }

        public override void GetActiveBlocks(ref TargetActiveBlockContext context)
        {
            context.AddBlock(BlockFields.SurfaceDescription.Alpha, (target.surfaceType == SurfaceType.Transparent || target.alphaClip) || target.allowMaterialOverride);
            context.AddBlock(BlockFields.SurfaceDescription.AlphaClipThreshold, target.alphaClip || target.allowMaterialOverride);
            context.AddBlock(CustomLitFields.SurfaceDescription.FogColor, m_FogMode == CustomFeatureModes.Custom);
            context.AddBlock(CustomLitFields.SurfaceDescription.FogIntensity, m_FogMode == CustomFeatureModes.Custom);
        }

        public override void CollectShaderProperties(PropertyCollector collector, GenerationMode generationMode)
        {
            if (target.allowMaterialOverride)
            {
                collector.AddFloatProperty(Property.CastShadows, target.castShadows ? 1.0f : 0.0f);
                collector.AddFloatProperty(Property.SurfaceType, (float)target.surfaceType);
                collector.AddFloatProperty(Property.BlendMode, (float)target.alphaMode);
                collector.AddFloatProperty(Property.AlphaClip, target.alphaClip ? 1.0f : 0.0f);
                collector.AddFloatProperty(Property.SrcBlend, 1.0f);    // always set by material inspector
                collector.AddFloatProperty(Property.DstBlend, 0.0f);    // always set by material inspector
                collector.AddToggleProperty(Property.ZWrite, (target.surfaceType == SurfaceType.Opaque));
                collector.AddFloatProperty(Property.ZWriteControl, (float)target.zWriteControl);
                collector.AddFloatProperty(Property.ZTest, (float)target.zTestMode);    // ztest mode is designed to directly pass as ztest
                collector.AddFloatProperty(Property.CullMode, (float)target.renderFace);    // render face enum is designed to directly pass as a cull mode

                bool enableAlphaToMask = (target.alphaClip && (target.surfaceType == SurfaceType.Opaque));
                collector.AddFloatProperty(Property.AlphaToMask, enableAlphaToMask ? 1.0f : 0.0f);
            }

            // We always need these properties regardless of whether the material is allowed to override other shader properties.
            // Queue control & offset enable correct automatic render queue behavior.  Control == 0 is automatic, 1 is user-specified.
            // We initialize queue control to -1 to indicate to UpdateMaterial that it needs to initialize it properly on the material.
            collector.AddFloatProperty(Property.QueueOffset, 0.0f);
            collector.AddFloatProperty(Property.QueueControl, -1.0f);
        }

        public override void GetPropertiesGUI(ref TargetPropertyGUIContext context, Action onChange, Action<String> registerUndo)
        {
            var universalTarget = (target as UniversalTarget);
            universalTarget.AddDefaultMaterialOverrideGUI(ref context, onChange, registerUndo);

            context.AddProperty("Additional Lights", new Toggle() { value = m_EnableAdditionalLights }, (evt) =>
            {
                if (Equals(m_EnableAdditionalLights, evt.newValue))
                    return;

                registerUndo("Change Additional Lights");
                m_EnableAdditionalLights = evt.newValue;
                onChange();
            });

            context.AddProperty("Light Cookies", new Toggle() { value = m_EnableLightCookies }, (evt) =>
            {
                if (Equals(m_EnableLightCookies, evt.newValue))
                    return;

                registerUndo("Change Light Cookies");
                m_EnableLightCookies = evt.newValue;
                onChange();
            });

            context.AddProperty("Fog Mode", new EnumField(m_FogMode) { }, (evt) =>
            {
                if (Equals(m_FogMode, evt.newValue))
                    return;

                registerUndo("Change Fog Mode");
                m_FogMode = (CustomFeatureModes)evt.newValue;
                onChange();
            });

            universalTarget.AddDefaultSurfacePropertiesGUI(ref context, onChange, registerUndo, showReceiveShadows: true);
        }

        #region SubShader
        static class SubShaders
        {
            public static SubShaderDescriptor CustomLit(UniversalTarget target, string renderType, string renderQueue, string disableBatchingTag,
                bool enableAdditionalLights, bool enableLightCookies, CustomFeatureModes fogMode)
            {
                var result = new SubShaderDescriptor()
                {
                    pipelineTag = UniversalTarget.kPipelineTag,
                    customTags = UniversalTarget.kUnlitMaterialTypeTag,
                    renderType = renderType,
                    renderQueue = renderQueue,
                    disableBatchingTag = disableBatchingTag,
                    generatesPreview = true,
                    passes = new PassCollection()
                };

                result.passes.Add(CustomLitPasses.Forward(target, enableAdditionalLights, enableLightCookies, fogMode));

                if (target.mayWriteDepth)
                    result.passes.Add(PassVariant(CorePasses.DepthOnly(target), CorePragmas.Instanced));

                result.passes.Add(PassVariant(CustomLitPasses.DepthNormalOnly(target), CorePragmas.Instanced));

                if (target.castShadows || target.allowMaterialOverride)
                    result.passes.Add(PassVariant(CorePasses.ShadowCaster(target), CorePragmas.Instanced));

                // Currently neither of these passes (selection/picking) can be last for the game view for
                // UI shaders to render correctly. Verify [1352225] before changing this order.
                result.passes.Add(PassVariant(CorePasses.SceneSelection(target), CorePragmas.Default));
                result.passes.Add(PassVariant(CorePasses.ScenePicking(target), CorePragmas.Default));
                result.passes.Add(PassVariant(CorePasses.MotionVectors(target), CorePragmas.Default));

                return result;
            }
        }
        #endregion

        static class CustomLitFields
        {
            [GenerateBlocks]
            public struct SurfaceDescription
            {
                public static string name = "SurfaceDescription";
                public static BlockFieldDescriptor FogColor = new BlockFieldDescriptor(SurfaceDescription.name, "Fog Color", "_USE_CUSTOM_FOG", new ColorControl(Color.white, false), ShaderStage.Fragment);
                public static BlockFieldDescriptor FogIntensity = new BlockFieldDescriptor(SurfaceDescription.name, "Fog Intensity", "_USE_CUSTOM_FOG", new Vector3Control(Vector3.zero), ShaderStage.Fragment);
            }
        }

        #region Pass
        static class CustomLitPasses
        {
            static void AddReceiveShadowsControlToPass(ref PassDescriptor pass, UniversalTarget target, bool receiveShadows)
            {
                if (target.allowMaterialOverride)
                    pass.keywords.Add(CustomLitKeywords.ReceiveShadowsOff);
                else if (!receiveShadows)
                    pass.defines.Add(CustomLitKeywords.ReceiveShadowsOff, 1);
            }

            public static PassDescriptor Forward(UniversalTarget target, bool enableAdditionalLights, bool enableLightCookies, CustomFeatureModes fogMode)
            {
                var result = new PassDescriptor
                {
                    // Definition
                    displayName = "Universal Forward",
                    referenceName = "SHADERPASS_UNLIT",
                    useInPreview = true,

                    // Template
                    passTemplatePath = UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = UniversalTarget.kSharedTemplateDirectories,

                    // Port Mask
                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = CustomLitBlockMasks.FragmentColorAlpha,

                    // Fields
                    structs = CoreStructCollections.Default,
                    requiredFields = CustomLitRequiredFields.Unlit,
                    fieldDependencies = CoreFieldDependencies.Default,

                    // Conditional State
                    renderStates = CoreRenderStates.UberSwitchedRenderState(target),
                    pragmas = new PragmaCollection { CorePragmas.Instanced },
                    defines = new DefineCollection { },
                    keywords = new KeywordCollection { CustomLitKeywords.Forward },
                    includes = new IncludeCollection { CustomLitIncludes.Unlit },

                    // Custom Interpolator Support
                    customInterpolators = CoreCustomInterpDescriptors.Common
                };

                CorePasses.AddTargetSurfaceControlsToPass(ref result, target);
                CorePasses.AddAlphaToMaskControlToPass(ref result, target);
                AddReceiveShadowsControlToPass(ref result, target, target.receiveShadows);
                CorePasses.AddLODCrossFadeControlToPass(ref result, target);

                if (fogMode == CustomFeatureModes.Standard)
                {
                    result.pragmas.Add(Pragma.MultiCompileFog);
                    result.defines.Add(CoreDefines.UseFragmentFog);
                }
                else if (fogMode == CustomFeatureModes.Custom)
                {
                    result.defines.Add(CustomLitKeywords.UseCustomFog, 1);
                }

                if (target.receiveShadows)
                {
                    result.keywords.Add(CoreKeywordDescriptors.MainLightShadows);
                }

                if (enableAdditionalLights)
                {
                    result.keywords.Add(CoreKeywordDescriptors.AdditionalLights);
                    result.keywords.Add(CoreKeywordDescriptors.AdditionalLightShadows);
                }

                if (enableLightCookies)
                {
                    result.keywords.Add(CoreKeywordDescriptors.LightCookies);
                }

                return result;
            }

            public static PassDescriptor DepthNormalOnly(UniversalTarget target)
            {
                var result = new PassDescriptor
                {
                    // Definition
                    displayName = "DepthNormalsOnly",
                    referenceName = "SHADERPASS_DEPTHNORMALSONLY",
                    lightMode = "DepthNormalsOnly",
                    useInPreview = false,

                    // Template
                    passTemplatePath = UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = UniversalTarget.kSharedTemplateDirectories,

                    // Port Mask
                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = CustomLitBlockMasks.FragmentDepthNormals,

                    // Fields
                    structs = CoreStructCollections.Default,
                    requiredFields = CustomLitRequiredFields.DepthNormalsOnly,
                    fieldDependencies = CoreFieldDependencies.Default,

                    // Conditional State
                    renderStates = CoreRenderStates.DepthNormalsOnly(target),
                    pragmas = CorePragmas.Forward,
                    defines = new DefineCollection(),
                    keywords = new KeywordCollection { CoreKeywordDescriptors.GBufferNormalsOct },
                    includes = new IncludeCollection { CoreIncludes.DepthNormalsOnly },

                    // Custom Interpolator Support
                    customInterpolators = CoreCustomInterpDescriptors.Common
                };

                CorePasses.AddTargetSurfaceControlsToPass(ref result, target);
                CorePasses.AddLODCrossFadeControlToPass(ref result, target);

                return result;
            }

            #region PortMasks
            static class CustomLitBlockMasks
            {
                public static readonly BlockFieldDescriptor[] FragmentColorAlpha = new BlockFieldDescriptor[]
                {
                    BlockFields.SurfaceDescription.BaseColor,
                    BlockFields.SurfaceDescription.Alpha,
                    BlockFields.SurfaceDescription.AlphaClipThreshold,
                    CustomLitFields.SurfaceDescription.FogColor,
                    CustomLitFields.SurfaceDescription.FogIntensity,
                };
                public static readonly BlockFieldDescriptor[] FragmentDepthNormals = new BlockFieldDescriptor[]
                {
                    BlockFields.SurfaceDescription.NormalWS,
                    BlockFields.SurfaceDescription.Alpha,
                    BlockFields.SurfaceDescription.AlphaClipThreshold,
                };
            }
            #endregion

            #region RequiredFields
            static class CustomLitRequiredFields
            {
                public static readonly FieldCollection Unlit = new FieldCollection()
                {
                    StructFields.Varyings.positionWS,
                    StructFields.Varyings.normalWS
                };

                public static readonly FieldCollection DepthNormalsOnly = new FieldCollection()
                {
                    StructFields.Varyings.normalWS,
                };
            }
            #endregion
        }
        #endregion

        #region Keywords
        static class CustomLitKeywords
        {
            public static readonly KeywordDescriptor UseCustomFog = new KeywordDescriptor()
            {
                displayName = "Use Custom Fog",
                referenceName = "_USE_CUSTOM_FOG",
                type = KeywordType.Boolean,
                definition = KeywordDefinition.ShaderFeature,
                scope = KeywordScope.Local,
            };

            public static readonly KeywordDescriptor ReceiveShadowsOff = new KeywordDescriptor()
            {
                displayName = "Receive Shadows Off",
                referenceName = "_RECEIVE_SHADOWS_OFF",
                type = KeywordType.Boolean,
                definition = KeywordDefinition.ShaderFeature,
                scope = KeywordScope.Local,
            };

            public static readonly KeywordCollection Forward = new KeywordCollection()
            {
                // This contain lightmaps because without a proper custom lighting solution in Shadergraph,
                // people start with the unlit then add lightmapping nodes to it.
                // If we removed lightmaps from the unlit target this would ruin a lot of peoples days.
                CoreKeywordDescriptors.LightmapShadowMixing,
                CoreKeywordDescriptors.StaticLightmap,
                CoreKeywordDescriptors.DirectionalLightmapCombined,
                CoreKeywordDescriptors.SampleGI,
                //CoreKeywordDescriptors.DBuffer,
                CoreKeywordDescriptors.DebugDisplay,
                //CoreKeywordDescriptors.ScreenSpaceAmbientOcclusion,
            };
        }
        #endregion

        #region Includes
        static class CustomLitIncludes
        {
            const string kShadows = "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl";
            const string kCustomLitPass = "Assets/NorthStar/Shaders/Editor/ShaderGraph/Includes/CustomLitPass.hlsl";

            public static IncludeCollection Unlit = new IncludeCollection
            {
                // Pre-graph
                { CoreIncludes.DOTSPregraph },
                //{ CoreIncludes.WriteRenderLayersPregraph },
                { CoreIncludes.CorePregraph },
                { kShadows, IncludeLocation.Pregraph },
                { CoreIncludes.ShaderGraphPregraph },
                { CoreIncludes.DBufferPregraph },

                // Post-graph
                { CoreIncludes.CorePostgraph },
                { kCustomLitPass, IncludeLocation.Postgraph },
            };
        }
        #endregion
    }
}
