// Copyright (c) Meta Platforms, Inc. and affiliates.

#ifndef CHARACTER_LIGHTING_INCLUDED
#define CHARACTER_LIGHTING_INCLUDED

#ifdef __INTELLISENSE__
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #define _ADDITIONAL_LIGHTS

    Texture2D<float> _MainLightShadowmapTexture;
    SamplerComparisonState sampler_LinearClampCompare;
#endif

SamplerState _LinearClampSampler, _PointClampSampler;
float4 _MainLightShadowmapTexture_TexelSize;

#ifndef SHADERGRAPH_PREVIEW
float3 CustomLightHandling(float3 lightColor, float3 L, float3 N, float3 albedo, float3 V, float roughness, float3 f0, float wrap, float3 subsurfaceColor) 
{
    float NdotL = dot(N, L);
    float clampedNdotL = saturate(NdotL);
    float NdotV = saturate(dot(N, V));
    
    // Optimized math. Ref: PBR Diffuse Lighting for GGX + Smith Microsurfaces (slide 114), assuming |L|=1 and |V|=1
    float LdotV = saturate(dot(L, V));
    float invLenLV = rsqrt(2.0 * LdotV + 2.0);
    float NdotH = saturate((NdotL + NdotV) * invLenLV);
    float LdotH = saturate(invLenLV * LdotV + invLenLV);
    
    // https://knarkowicz.wordpress.com/2018/01/04/cloth-shading/
    float invAlpha = rcp(max(1e-3, roughness));
    float sin2h = saturate(1.0 - NdotH * NdotH);
    float D = (2.0 + invAlpha) * pow(sin2h, invAlpha * 0.5) / (2.0 * PI);
    float Vis = rcp(4.0 * (NdotL + NdotV - NdotL * NdotV));
    
    float f = pow(1.0 - LdotH, 5.0);
    float3 F = f + f0 * (1.0 - f);
    
    float3 specular =  F * D * Vis * PI;
    
    float3 fd = albedo * saturate((NdotL + wrap) * rcp(Sq(1.0 + wrap))) * saturate(subsurfaceColor + abs(NdotL));
    
    return (fd + specular * clampedNdotL) * lightColor;
}
#endif

void ClothLighting_float(float3 Position, float3 Normal, float3 ViewDirection, float3 Albedo, float Smoothness, float3 Specular, float wrap, float3 subsurfaceColor, out float3 Color)
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
    
    float perceptualRoughness = 1.0 - Smoothness;
    float roughness = perceptualRoughness * perceptualRoughness;
    Color = CustomLightHandling(mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation, mainLight.direction, Normal, Albedo, ViewDirection, roughness, Specular, wrap, subsurfaceColor);
    
    #ifdef _ADDITIONAL_LIGHTS
        // Shade additional lights if enabled
        uint numAdditionalLights = GetAdditionalLightsCount();
        for (uint i = 0; i < numAdditionalLights; i++) 
        {
            Light light = GetAdditionalLight(i, Position, 1.0);
            Color += CustomLightHandling(light.color * light.distanceAttenuation * light.shadowAttenuation, light.direction, Normal, Albedo, ViewDirection, roughness, Specular, wrap, subsurfaceColor);
        }
    #endif
#endif
    
}
#endif