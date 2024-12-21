// Copyright (c) Meta Platforms, Inc. and affiliates.

#ifndef CHARACTER_LIGHTING_INCLUDED
#define CHARACTER_LIGHTING_INCLUDED

#pragma target 4.5

#ifdef __INTELLISENSE__
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #define _ADDITIONAL_LIGHTS

    Texture2D<float> _MainLightShadowmapTexture;
    SamplerComparisonState sampler_LinearClampCompare;
#endif

SamplerState _LinearClampSampler, _PointClampSampler;
float4 _MainLightShadowmapTexture_TexelSize;

// Gives weights for four texels from a 0-1 input position to match a gather result
float4 BilinearWeights(float2 uv, float2 textureSize)
{
    float2 offset = rcp(512.0) - 0.5;
    float2 localUv = frac(uv * textureSize + offset);
    float4 weights = localUv.xxyy * float2(-1, 1).xyyx + float2(1, 0).xyyx;
    return weights.zzww * weights.xyyx;
}

// Computes the scalar specular term for Minimalist CookTorrance BRDF
// NOTE: needs to be multiplied with reflectance f0, i.e. specular color to complete
float3 DirectBRDFSpecular(float3 N, float3 L, float3 V, float roughness, float3 f0)
{
    float NdotV = saturate(dot(N, V));
    float a2 = roughness * roughness;
    float NdotL = saturate(dot(N, L));
    
    // Optimized math. Ref: PBR Diffuse Lighting for GGX + Smith Microsurfaces (slide 114), assuming |L|=1 and |V|=1
    float LdotV = saturate(dot(L, V));
    float invLenLV = rsqrt(2.0 * LdotV + 2.0);
    float NdotH = saturate((NdotL + NdotV) * invLenLV);
    float LdotH = saturate(invLenLV * LdotV + invLenLV);

    float d = Sq(NdotH) * (a2 - 1.0) + 1.0;
    return a2 * rcp(Sq(d) * Sq(LdotH) * (roughness * 4.0 + 2.0)) * f0;
}

#ifndef SHADERGRAPH_PREVIEW
float3 CustomLightHandling(float3 lightColor, float shadow, float3 L, float3 N, float3 albedo, float Curvature, float3 transmittance, Texture2D<float4> SkinLUT, float3 V, float roughness, float3 f0, float Metallic) 
{
    float NdotL = dot(N, L);
    float3 scatter = SkinLUT.SampleLevel(_LinearClampSampler, float2(0.5 * NdotL + 0.5, Curvature), 0.0).rgb;
    return (albedo * (transmittance * saturate(-NdotL) + scatter * shadow) * (1-Metallic) + DirectBRDFSpecular(N, L, V, roughness, f0) * saturate(NdotL) * shadow) * lightColor;
}
#endif

void SkinLighting_float(float3 Position, float3 Normal, float3 ViewDirection, float3 Albedo, float Smoothness, float Curvature, float3 thicknessScale, float Metallic, Texture2D<float4> SkinLUT, float bias, float normalBias, float occlusion, float microShadow, out float3 Color)
{
#ifdef SHADERGRAPH_PREVIEW
    Color = 1;
#else
    
#ifdef _MAIN_LIGHT_SHADOWS_CASCADE
    half cascadeIndex = ComputeCascadeIndex(Position);
#else
    half cascadeIndex = 0;
#endif
    float3 shadowCoord = mul(_MainLightWorldToShadow[cascadeIndex], float4(Position, 1.0)).xyz;
    Light mainLight = GetMainLight(float4(shadowCoord, 1), Position, 0.0);
    
    float3 shadowCoord1 = mul(_MainLightWorldToShadow[cascadeIndex], float4(Position - Normal * normalBias, 1.0)).xyz;
    
    #if 0
        // Perform 1 comparison of a bilinearly-weighted depth (Faster but more artifact-prone)
        float shadowDepth = _MainLightShadowmapTexture.Sample(_LinearClampSampler, shadowCoord1.xy).r;
        float3 transmittance = exp(-max(0.0, ((shadowDepth - shadowCoord.z + bias) * _MainLightShadowDepthScales[cascadeIndex])) * thicknessScale);
    #else
        // Perform 4 comparisons and bilinearly weigh them
        float4 shadowDepths = _MainLightShadowmapTexture.Gather(_LinearClampSampler, shadowCoord1.xy);
        shadowDepths = max(0.0, (shadowDepths - shadowCoord.z + bias) * _MainLightShadowDepthScales[cascadeIndex]);
        float shadowDepth = dot(shadowDepths, BilinearWeights(shadowCoord1.xy, _MainLightShadowmapTexture_TexelSize.zw));
        float3 transmittance = exp(-shadowDepth * thicknessScale);
    #endif
    
    float perceptualRoughness = 1.0 - Smoothness;
    float roughness = perceptualRoughness * perceptualRoughness;
    float3 specular = lerp(0.04, Albedo, Metallic);
    Color = CustomLightHandling(mainLight.color * mainLight.distanceAttenuation, mainLight.shadowAttenuation * ComputeMicroShadowing(occlusion, dot(Normal, mainLight.direction), microShadow), mainLight.direction, Normal, Albedo, Curvature, transmittance, SkinLUT, ViewDirection, roughness, specular, Metallic);
    
    #ifdef _ADDITIONAL_LIGHTS
        // Shade additional lights if enabled
        uint numAdditionalLights = GetAdditionalLightsCount();
        for (uint i = 0; i < numAdditionalLights; i++) 
        {
            Light light = GetAdditionalLight(i, Position, 1.0);
            float3 transmittance = 0.0; // no transmittance for point lights due to no shadow maps
            Color += CustomLightHandling(light.color * light.distanceAttenuation, light.shadowAttenuation, light.direction, Normal, Albedo, Curvature, transmittance, SkinLUT, ViewDirection, roughness, specular, Metallic);
        }
    #endif
#endif
    
}
#endif