// Copyright (c) Meta Platforms, Inc. and affiliates.

#ifndef PBR_LIGHTING_MAIN_LIGHT_OVERRIDE
#define PBR_LIGHTING_MAIN_LIGHT_OVERRIDE

#include "PbrLighting.hlsl"

void PbrLightingMainLightOverride_float(float3 Position, float3 Normal, float3 ViewDirection, float3 Albedo, float Smoothness, float Metallic, float3 BakedGI, float3 LightColor, out float3 Color)
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
    float roughness = max(perceptualRoughness * perceptualRoughness, HALF_MIN_SQRT);
    float3 specularColor = lerp(0.04, Albedo, Metallic);
    Color = CustomLightHandling(LightColor * mainLight.shadowAttenuation, mainLight.direction, Normal, Albedo, ViewDirection, roughness, specularColor);
    
    #ifdef _ADDITIONAL_LIGHTS
        // Shade additional lights if enabled
        uint numAdditionalLights = GetAdditionalLightsCount();
        for (uint i = 0; i < numAdditionalLights; i++) 
        {
            Light light = GetAdditionalLight(i, Position, 1.0);
            Color += CustomLightHandling(light.color * light.distanceAttenuation * light.shadowAttenuation, light.direction, Normal, Albedo, ViewDirection, roughness, specularColor);
        }
    #endif
#endif
    
}
#endif