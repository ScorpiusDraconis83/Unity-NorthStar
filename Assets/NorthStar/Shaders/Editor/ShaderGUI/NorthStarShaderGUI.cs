// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NorthStar
{
    /// <summary>
    /// The default GUI for most shaders in North Star*
    /// </summary>
    public class NorthStarShaderGUI : ShaderGUI
    {
        [Flags]
        public enum ShaderType
        {
            None = 0,
            Default = 1 << 0, // PBRDEFAULT
            Normal = 1 << 1, // PBRN
            NormalSmooth = 1 << 2, // PBRNS
            NormalSmoothParallax = 1 << 3, // PBRNSPARALLAX
            NormalSmoothMetal = 1 << 4, // PBRNSM
            Foliage = 1 << 5, // PBRFOLIAGE
            UnderwaterNormal = 1 << 6, // PBRUDRWRN
            UnderwaterNormalSmoothness = 1 << 7, // PBRUDRWRNS
            UnderwaterFoliage = 1 << 8 // PBRUDRWRFOLIAGE
        }

        public class ShaderConfig
        {
            public string Path { get; }
            public string DisplayName { get; }
            public ShaderType Type { get; }
            public bool SupportsDetail { get; }
            public bool SupportsSpecFilter { get; }
            public bool SupportsWind { get; }
            public bool SupportsFoliage { get; }
            public bool SupportParallax { get; }
            public bool SupportSmoothness { get; }
            public bool SupportUnderwater { get; }

            public ShaderConfig(string path, string displayName, ShaderType type, bool supportsDetail = true,
                bool supportsSpecFilter = true, bool supportsWind = false, bool supportsFoliage = false,
                bool supportParallax = false, bool supportSmoothness = false, bool supportsUnderwater = false)

            {
                Path = path;
                DisplayName = displayName;
                Type = type;
                SupportsDetail = supportsDetail;
                SupportsSpecFilter = supportsSpecFilter;
                SupportsWind = supportsWind;
                SupportsFoliage = supportsFoliage;
                SupportParallax = supportParallax;
                SupportSmoothness = supportSmoothness;
                SupportUnderwater = supportsUnderwater;
            }
        }

        private static readonly ShaderConfig[] s_shaderConfigs =
        {
            new(
                "NorthStar/PBR/PBR_Default",
                "PRB Default - Don't Use",
                ShaderType.Default,
                true,
                true,
                false,
                false,
                true,
                false,
                false
            ),
            new(
                "NorthStar/PBR/PBR_N",
                "PBR Basic",
                ShaderType.Normal,
                true,
                false
            ),
            new(
                "NorthStar/PBR/PBR_NS",
                "PBR with Smoothness",
                ShaderType.NormalSmooth
                ),
            new(
                "NorthStar/PBR/PBR_NS_Parallax",
                "PBR with Smoothness and Parallax",
                ShaderType.NormalSmoothParallax,
                supportParallax: true
            ),
            new(
                "NorthStar/PBR/PBR_NSM",
                "PBR with Smoothness & Metallic",
                ShaderType.NormalSmoothMetal
            ),
            new(
                "NorthStar/PBR/Foliage/PBR_Foliage",
                "PBR Foliage",
                ShaderType.Foliage,
                false,
                supportsWind: true,
                supportsFoliage: true,
                supportsSpecFilter: false,
                supportParallax: false
            ),
            new(
                "NorthStar/PBR/PBR_N_Underwater",
                "PBR Underwater with Normal",
                ShaderType.UnderwaterNormal,
                false,
                false,
                supportParallax: false,
                supportsUnderwater:true
            ),
            new(
                "NorthStar/PBR/PBR_NS_Underwater",
                "PBR Underwater with Normal & Smoothness",
                ShaderType.UnderwaterNormalSmoothness,
                false,
                false,
                supportsUnderwater:true,
                supportSmoothness:true
            ),
            new(
                "NorthStar/PBR/Foliage/PBR_FoliageUnderwater",
                "PBR Underwater Foliage",
                ShaderType.UnderwaterFoliage,
                false,
                supportsWind: true,
                supportsFoliage: true,
                supportsSpecFilter: false,
                supportsUnderwater:true
                )
        };

        private static readonly string[] s_renderingModes = { "Opaque", "Cutout", "Transparent" };

        //private static readonly int _NormalTexture = Shader.PropertyToID("_NormalTexture");
        private int m_selectedRenderingMode;
        private int m_selectedShaderIndex;
        private bool m_showInfo;

        private void DrawShaderSelector(MaterialEditor editor)
        {
            var material = editor.target as Material;

            // Find current shader config
            ShaderConfig currentConfig = null;
            for (var i = 0; i < s_shaderConfigs.Length; i++)
            {
                if (s_shaderConfigs[i].Path == material.shader.name)
                {
                    currentConfig = s_shaderConfigs[i];
                    m_selectedShaderIndex = i;
                    break;
                }
            }

            if (currentConfig == null)
            {
                m_selectedShaderIndex = 0;
                _ = s_shaderConfigs[0];
            }

            EditorGUI.BeginChangeCheck();

            // Create display names array for popup
            var displayNames = new string[s_shaderConfigs.Length];
            for (var i = 0; i < s_shaderConfigs.Length; i++)
            {
                displayNames[i] = s_shaderConfigs[i].DisplayName;
            }

            m_selectedShaderIndex = EditorGUILayout.Popup("Selected Shader:", m_selectedShaderIndex, displayNames);

            if (EditorGUI.EndChangeCheck())
            {
                var newConfig = s_shaderConfigs[m_selectedShaderIndex];
                var newShader = Shader.Find(newConfig.Path);
                if (newShader != null)
                {
                    // Store important properties before changing shader
                    var oldRenderingMode = material.GetFloat("_RENDERINGMODE");

                    // Change the shader
                    Undo.RecordObject(material, "Shader Change");
                    material.shader = newShader;

                    // Restore important properties
                    material.SetFloat("_RENDERINGMODE", oldRenderingMode);
                    SetupMaterialWithRenderingMode(material, (int)oldRenderingMode);
                }
            }
        }


        public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
        {
            var textureTooltip = "CTRL+Click to expand";
            var normalMapTooltip = "CTRL+Click to expand, Optimized when unused";
            var errorStyle = new GUIStyle(EditorStyles.label);
            errorStyle.normal.textColor = Color.red;
            errorStyle.hover.textColor = Color.red;

            var currentMaterial = editor.target as Material;
            _ = currentMaterial.shader.name;

            ShaderConfig currentConfig = null;

            for (var i = 0; i < s_shaderConfigs.Length; i++)
            {
                if (s_shaderConfigs[i].Path == currentMaterial.shader.name)
                {
                    currentConfig = s_shaderConfigs[i];
                    break;
                }
            }

            currentConfig ??= s_shaderConfigs[0];

            // Warnings
            if (currentConfig.Type == ShaderType.Default)
            {
                EditorGUILayout.HelpBox(
                    "This is an all options shader, consider using another shader from the dropdown, they have optimizations built-in for their use.",
                    MessageType.Error, true);
            }

            // Draw shader selector at the top
            DrawShaderSelector(editor);

            if (!currentConfig.SupportUnderwater)
            {
                //NorthStar is using the metallic workflow, force it to metallic
                var workflow = FindProperty("_WorkflowMode", properties);
                workflow.floatValue = 1f;
            }

            if (currentConfig.Type == ShaderType.Default)
            {
                GUILayout.Label("Default PBR Shader", EditorStyles.boldLabel);
            }

            if (currentConfig.Type == ShaderType.Normal)
            {
                GUILayout.Label("PBR Shader with only Normal support", EditorStyles.boldLabel);
            }

            if (currentConfig.Type == ShaderType.NormalSmooth)
            {
                GUILayout.Label("PBR Shader with Smoothness support", EditorStyles.boldLabel);
            }

            if (currentConfig.Type == ShaderType.NormalSmoothParallax)
            {
                GUILayout.Label("PBR Shader with Smoothness and parallax support", EditorStyles.boldLabel);
            }

            if (currentConfig.Type == ShaderType.NormalSmoothMetal)
            {
                GUILayout.Label("PBR Shader with Smoothness and Metallic support", EditorStyles.boldLabel);
            }

            if (currentConfig.Type == ShaderType.Foliage)
            {
                GUILayout.Label("PBR Shader for foliage", EditorStyles.boldLabel);
            }

            if (currentConfig.Type == ShaderType.UnderwaterNormal)
            {
                GUILayout.Label("PBR Shader for underwater", EditorStyles.boldLabel);
            }

            if (currentConfig.Type == ShaderType.UnderwaterNormalSmoothness)
            {
                GUILayout.Label("PBR Shader for underwater with smoothness", EditorStyles.boldLabel);
            }

            if (currentConfig.Type == ShaderType.UnderwaterFoliage)
            {
                GUILayout.Label("PBR Shader for underwater foliage", EditorStyles.boldLabel);
            }

            GUILayout.Space(10);

            //Material Overrides
            GUILayout.Label("Material Settings", EditorStyles.boldLabel);

            var renderingModeProp = FindProperty("_RENDERINGMODE", properties);
            m_selectedRenderingMode = (int)renderingModeProp.floatValue;
            m_selectedRenderingMode = EditorGUILayout.Popup("Rendering Mode", m_selectedRenderingMode, s_renderingModes);

            if (Mathf.Abs(renderingModeProp.floatValue - m_selectedRenderingMode) > 0.001f)
            {
                renderingModeProp.floatValue = m_selectedRenderingMode;

                // Apply rendering settings to all targeted materials
                foreach (Material material in editor.targets)
                {
                    SetupMaterialWithRenderingMode(material, m_selectedRenderingMode);
                }
            }

            if (m_selectedRenderingMode == 1) // Cutout mode only
            {
                var alphaClip = FindProperty("_AlphaClip", properties);
                editor.ShaderProperty(alphaClip, "Alpha Clip Threshold");
            }

            if (m_selectedRenderingMode == 2) // Transparency mode only
            {
                var zWrite = FindProperty("_ZWrite", properties);
                editor.ShaderProperty(zWrite, "Depth Write");
            }

            if (!currentConfig.SupportUnderwater)
            {
                //Shadow casting and receiving toggles
                var castShadows = FindProperty("_CastShadows", properties);
                var receiveShadows = FindProperty("_ReceiveShadows", properties);

                EditorGUI.BeginChangeCheck();
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Cast Shadows", GUILayout.Width(EditorGUIUtility.labelWidth));
                    castShadows.floatValue = EditorGUILayout.Toggle(castShadows.floatValue == 1) ? 1 : 0;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Receive Shadows", GUILayout.Width(EditorGUIUtility.labelWidth));
                    receiveShadows.floatValue = EditorGUILayout.Toggle(receiveShadows.floatValue == 1) ? 1 : 0;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    foreach (Material material in editor.targets)
                    {
                        material.SetShaderPassEnabled("ShadowCaster", material.GetFloat("_CastShadows") == 1f);
                        CoreUtils.SetKeyword(material, ShaderKeywordStrings._RECEIVE_SHADOWS_OFF, material.GetFloat("_ReceiveShadows") == 0.0f);
                    }
                }

                var cullMode = FindProperty("_Cull", properties);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Render Back", GUILayout.Width(EditorGUIUtility.labelWidth));
                    cullMode.floatValue = EditorGUILayout.Toggle(cullMode.floatValue == 0) ? 0 : 2;
                }
            }

            GUILayout.Space(10);

            //Textures
            GUILayout.Label("Textures", EditorStyles.boldLabel);

            var albedoTexture = FindProperty("_AlbedoTexture", properties);
            var normalTexture = FindProperty("_NormalTexture", properties);
            var disableNormalTexture = FindProperty("_DISABLENORMALTEXTURE", properties);
            // var _IS_STANDARD_NORMAL_TEXTURE = FindProperty("_IS_STANDARD_NORMAL_TEXTURE", properties); Switched out keyword for a bool
            var isStandardNormalTexture = FindProperty("_Is_Standard_Normal_Texture", properties);

            _ = editor.TexturePropertyMiniThumbnail(EditorGUILayout.GetControlRect(), albedoTexture, "Albedo Texture", textureTooltip);
            GUILayout.Label("Channels: RGB: Color, A: EmissiveMask/OpacityMask", EditorStyles.miniLabel);

            _ = editor.TexturePropertyMiniThumbnail(EditorGUILayout.GetControlRect(), normalTexture, "Normal Texture", normalMapTooltip);
            GUILayout.Label("Channels: RG: NormalMap, B: Smoothness A: Metallic/AmbientOcclusion", EditorStyles.miniLabel);

            if (normalTexture.textureValue != null)
            {
                disableNormalTexture.floatValue = 0;

                // Create horizontal layout for the checkbox and convert button
                using (new EditorGUILayout.HorizontalScope())
                {
                    editor.ShaderProperty(isStandardNormalTexture, "Using Standard Normal Texture");

                    // Only show the convert button if we're not using standard normal texture
                    if (isStandardNormalTexture.floatValue == 1)
                    {
                        isStandardNormalTexture.floatValue = 1;
                    }
                }
            }
            else
            {
                disableNormalTexture.floatValue = 1;
            }

            CheckNormalMapSettings(normalTexture, isStandardNormalTexture);

            GUILayout.Space(10);

            //Tiling
            var tiling = FindProperty("_Tiling", properties);
            var tilingValue = tiling.vectorValue;
            var tilingVector2 = EditorGUILayout.Vector2Field(
                "Texture Tiling", new Vector2(tilingValue.x, tilingValue.y));
            tiling.vectorValue = new Vector4(tilingVector2.x, tilingVector2.y, 0, 0);

            if ((currentConfig.Type & (ShaderType.Default | ShaderType.NormalSmooth)) != 0)
            {
                var offset = FindProperty("_Offset", properties);
                var offsetValue = offset.vectorValue;
                var offsetVector2 = EditorGUILayout.Vector2Field(
                    "Texture Offset", new Vector2(offsetValue.x, offsetValue.y));
                offset.vectorValue = new Vector4(offsetVector2.x, offsetVector2.y, 0, 0);
            }

            GUILayout.Space(10);

            // Material Parameters
            if (currentConfig.Type == ShaderType.Default)
            {
                GUILayout.Label("Material Parameters", EditorStyles.boldLabel);

                // Normal Parameters
                var normalValue = FindProperty("_NormalValue", properties);
                editor.ShaderProperty(normalValue, "Normal Value");

                // Smoothness Parameters
                var useSmoothnessMap = FindProperty("_UseSmoothnessMap", properties);
                editor.ShaderProperty(useSmoothnessMap, "Use Smoothness Map");
                var smoothnessValue = FindProperty("_SmoothnessValue", properties);
                editor.ShaderProperty(smoothnessValue, "Smoothness Value");

                // Metallic Parameters
                var useMetallicMap = FindProperty("_UseMetallicMap", properties);
                editor.ShaderProperty(useMetallicMap, "Use Metallic Map");
                var metallicValue = FindProperty("_MetallicValue", properties);
                editor.ShaderProperty(metallicValue, "Metallic Value");

                // Ambient Occlusion Parameters
                var useAmbientOcclustionmap = FindProperty("_UseAmbientOcclustionmap", properties);
                editor.ShaderProperty(useAmbientOcclustionmap, "Use Ambient Occlusion Map");
                var ambientOcclusionValue = FindProperty("_AmbientOcclusionValue", properties);
                editor.ShaderProperty(ambientOcclusionValue, "Ambient Occlusion Value");

                // Opacity Parameters
                var useOpacityMap = FindProperty("_UseOpacityMap", properties);
                editor.ShaderProperty(useOpacityMap, "Use Opacity Map");
                var opacityValue = FindProperty("_OpacityValue", properties);
                editor.ShaderProperty(opacityValue, "Opacity Value");

                // Emissive Parameters
                var useEmissiveMaskMap = FindProperty("_UseEmissiveMaskMap", properties);
                editor.ShaderProperty(useEmissiveMaskMap, "Use Emissive Mask Map");
                var emissiveMaskValue = FindProperty("_EmissiveMaskValue", properties);
                editor.ShaderProperty(emissiveMaskValue, "Emissive Mask Value");
                var emissiveColor = FindProperty("_EmissiveColor", properties);
                editor.ShaderProperty(emissiveColor, "Emissive Color");

                GUILayout.Space(10);
            }

            //Foliage Parameters
            if ((currentConfig.Type & (ShaderType.Foliage | ShaderType.UnderwaterFoliage)) != 0)
            {
                // GUILayout.Label("Subsurface Scattering - It's fake", EditorStyles.boldLabel);
                // var _USE_FAKE_SUBSURFACESCATTERING = FindProperty("_USE_FAKE_SUBSURFACESCATTERING", properties);
                // editor.ShaderProperty(_USE_FAKE_SUBSURFACESCATTERING, "Use Subsurface Scattering");
                //
                // if (_USE_FAKE_SUBSURFACESCATTERING.floatValue == 1)
                // {
                //     var thicknessMap = FindProperty("_ThicknessMap", properties);
                //     _ = editor.TexturePropertySingleLine(new GUIContent("Thickness Map"), thicknessMap);
                //
                //     var sSSColor = FindProperty("_SSSColor", properties);
                //     editor.ShaderProperty(sSSColor, "SSS Color");
                //
                //     var colorVariation = FindProperty("_Color_Variation", properties);
                //     editor.ShaderProperty(colorVariation, "Color Variation");
                //
                //     var hotspot = FindProperty("_Hotspot", properties);
                //     editor.ShaderProperty(hotspot, "Hotspot");
                //
                //     var strength = FindProperty("_Strength", properties);
                //     editor.ShaderProperty(strength, "Strength");
                //
                //     GUILayout.Label("R channel vertex colors impact wind intensity", EditorStyles.miniLabel);
                //
                //     GUILayout.Space(10);
                // }

                GUILayout.Label("Foliage Tinting", EditorStyles.boldLabel);
                var minLightTint = FindProperty("_MinLightTint", properties);
                editor.ShaderProperty(minLightTint, "Min Light Tint");

                var maxLightTint = FindProperty("_MaxLightTint", properties);
                editor.ShaderProperty(maxLightTint, "Max light Tint");

                var colorVariation = FindProperty("_Color_Variation", properties);
                editor.ShaderProperty(colorVariation, "Color Variation");

                var lightTintFactor = FindProperty("_LightTintFactor", properties);
                editor.ShaderProperty(lightTintFactor, "Light Tint Factor");

                var lightTintOffset = FindProperty("_LightTintOffset", properties);
                editor.ShaderProperty(lightTintOffset, "Light Tint Offset");


                GUILayout.Space(10);

                if ((currentConfig.Type & (ShaderType.Foliage | ShaderType.UnderwaterFoliage)) != 0)
                {
                    GUILayout.Label("Wind", EditorStyles.boldLabel);
                    var useWind = FindProperty("_USE_WIND", properties);
                    editor.ShaderProperty(useWind, "Enable Wind");
                    GUILayout.Label("Vertex Color R Channel", EditorStyles.miniLabel);

                    var individualLeafOffset = FindProperty("_Individual_Leaf_Offset", properties);
                    editor.ShaderProperty(individualLeafOffset, "Offset Individual Leaf");
                    GUILayout.Label("Vertex Color b Channel", EditorStyles.miniLabel);


                    GUILayout.Space(10);

                    if (useWind.floatValue == 1f)
                    {
                        var hasTrunk = FindProperty("_HAS_TRUNK", properties);
                        editor.ShaderProperty(hasTrunk, "Enable Trunk Wind");
                        GUILayout.Label("Vertex Color B Channel", EditorStyles.miniLabel);
                    }

                    GUILayout.Space(10);
                }
            }

            //Individual rain disable override toggle
            if (currentConfig.Type == ShaderType.NormalSmoothMetal)
            {
                GUILayout.Label("Rain Disable Override", EditorStyles.boldLabel);
                GUILayout.Label("Only needed if rain is globally enabled and you don't want this material to be affected", EditorStyles.miniLabel);
                editor.ShaderProperty(FindProperty("_Disable_Rain_Override", properties), "Disable Rain Override");
                GUILayout.Space(10);

            }

            //Detail Properties
            if (currentConfig.SupportsDetail)
            {
                GUILayout.Label("Detail Texture Mapping", EditorStyles.boldLabel);
                GUILayout.Label(
                    "Additional detail maps and effects, such as wetness and dirt", EditorStyles.miniLabel);
                var useTrimsheetDetails = FindProperty("_USE_TRIMSHEET_DETAILS", properties);
                editor.ShaderProperty(useTrimsheetDetails, "Enable Detail Texture Mapping");

                if (useTrimsheetDetails.floatValue == 1f)
                {
                    var detailTexture = FindProperty("_DetailTexture", properties);
                    _ = editor.TexturePropertyMiniThumbnail(
                        EditorGUILayout.GetControlRect(), detailTexture, "Detail Texture", textureTooltip);
                    GUILayout.Label(
                        "Channels: R: Ambient Occlusion, G: Wetness B: Dirt A: None", EditorStyles.miniLabel);
                    GUILayout.Label("These texture maps use the third UV channel", EditorStyles.miniLabel);

                    var wetnessSpreadMin = FindProperty("_WetnessSpreadMin", properties);
                    editor.ShaderProperty(wetnessSpreadMin, "Wetness Spread Min");

                    var wetnessSpreadMax = FindProperty("_WetnessSpreadMax", properties);
                    editor.ShaderProperty(wetnessSpreadMax, "Wetness Spread Max");

                    var wetnessSpreadOffset = FindProperty("_WetnessSpreadOffset", properties);
                    editor.ShaderProperty(wetnessSpreadOffset, "Wetness Spread Offset");

                    var wetnessSmoothingFactor = FindProperty("_WetnessSmoothingFactor", properties);
                    editor.ShaderProperty(wetnessSmoothingFactor, "Wetness Normal Smoothness");

                    var dirtMaskStrength = FindProperty("_DirtMaskStrength", properties);
                    editor.ShaderProperty(dirtMaskStrength, "Dirt Intensity");

                    GUILayout.Space(10);
                }
                GUILayout.Space(10);
            }

            if (currentConfig.SupportsSpecFilter)
            {
                // Specular filtering
                GUILayout.Label("Specular Filtering", EditorStyles.boldLabel);
                GUILayout.Label("Fade out shimmering hotspots in the distance", EditorStyles.miniLabel);
                var specularFilteringProp = FindProperty("_SPECULAR_FILTERING", properties);
                editor.ShaderProperty(specularFilteringProp, "Enable Specular Filtering");
                if (specularFilteringProp.floatValue == 1f)
                {
                    editor.ShaderProperty(
                        FindProperty("_SpecularAAVariance", properties), "Specular AA Variance");
                    editor.ShaderProperty(
                        FindProperty("_SpecularAAThreshold", properties), "Specular AA Threshold");
                }

                GUILayout.Space(10);
            }

            if (currentConfig.Type == ShaderType.UnderwaterNormal)
            {
                var useCausticts = FindProperty("_USE_CAUSTICTS", properties);
                editor.ShaderProperty(useCausticts, "Enable Caustics");
                GUILayout.Label("Use Vertex Alpha channel to mask out Caustics", EditorStyles.miniLabel);
                GUILayout.Space(10);
            }

            // Parralax mapping properties
            if (currentConfig.SupportParallax)
            {
                GUILayout.Label("Parallax mapping");

                var parallaxTexture = FindProperty("_Parallax_Texture", properties);
                _ = editor.TexturePropertySingleLine(new GUIContent("Parallax Texture"), parallaxTexture);

                var parallaxAmplitude = FindProperty("_Parallax_Amplitude", properties);
                editor.ShaderProperty(parallaxAmplitude, "Parallax Amplitude");
                GUILayout.Space(10);
            }

            _ = editor.DoubleSidedGIField();
            editor.RenderQueueField();
            GUILayout.Space(10);

            m_showInfo = EditorGUILayout.Foldout(m_showInfo, "Info", true);
            if (m_showInfo)
            {
                EditorGUILayout.LabelField(
                    "This multi shader approach helps reduce instruction and variant counts. Connected parameters in ShaderGraph compile as local and global variables preventing the shader compiler from optimizing and folding generated shader code into cheaper instructions. \n \nCustom UI is set Shader Graph's CustomEditorGUI field",
                    EditorStyles.helpBox);
                GUILayout.Space(10);
            }
        }


        private void CheckNormalMapSettings(MaterialProperty normalMapProperty,
            MaterialProperty usingNormalmapProperty)
        {
            if (normalMapProperty.textureValue != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(normalMapProperty.textureValue);
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (importer != null)
                {
                    if (importer.textureType == TextureImporterType.NormalMap || importer.sRGBTexture)
                    {
                        if (usingNormalmapProperty.floatValue == 0)
                        {
                            GUILayout.Space(10);
                            EditorGUILayout.HelpBox(
                                "If this is a standard Normal map texture please enable 'Using Standard Normal Texture' \n \nOtherwise if you need Smoothness and Metallic maps, the normal map should have it's 'Texture Type' set to 'Default' and 'sRGB (Color Texture)' turned off",
                                MessageType.Warning);
                            if (GUILayout.Button("Fix Texture Settings"))
                            {
                                importer.textureType = TextureImporterType.Default;
                                importer.sRGBTexture = false;
                                importer.SaveAndReimport();
                            }

                            GUILayout.Space(10);
                        }
                    }
                }
            }
        }

        private void SetupMaterialWithRenderingMode(Material material, int mode)
        {
            switch (mode)
            {
                case 0:
                    material.SetOverrideTag("RenderType", "Opaque");
                    material.renderQueue = (int)RenderQueue.Geometry;
                    material.SetInt("_SrcBlend", (int)BlendMode.One);
                    material.SetInt("_DstBlend", (int)BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.SetInt("_UseOpacityMap", 0);
                    material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.EnableKeyword("_SURFACE_TYPE_OPAQUE");

                    break;

                case 1: // Cutout
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.renderQueue = (int)RenderQueue.AlphaTest;
                    material.SetInt("_SrcBlend", (int)BlendMode.One);
                    material.SetInt("_DstBlend", (int)BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.SetInt("_UseOpacityMap", 1);
                    material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.EnableKeyword("_SURFACE_TYPE_OPAQUE");

                    break;

                case 2: // Transparent
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.renderQueue = (int)RenderQueue.Transparent;
                    material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.SetInt("_UseOpacityMap", 1);
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.DisableKeyword("_SURFACE_TYPE_OPAQUE");
                    break;
            }
        }
    }
}