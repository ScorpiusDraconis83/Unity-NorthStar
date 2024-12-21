// Copyright (c) Meta Platforms, Inc. and affiliates.

#ifndef CHARACTER_LIGHTING_INCLUDED
#define CHARACTER_LIGHTING_INCLUDED

#pragma target 4.5

#ifdef __INTELLISENSE__
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #define _ADDITIONAL_LIGHTS
#endif

float3 CustomLightHandling(float3 lightColor, float3 L, float3 N, float3 albedo, float3 V, float roughness, float3 f0) 
{
    float roughness2 = max(roughness * roughness, HALF_MIN);
    float normalizationTerm = roughness * 4.0h + 2.0h;
    
    float3 halfDir = normalize(L + V);

    float NoH = saturate(dot(N, halfDir));
    half LoH = saturate(dot(L, halfDir));

    // GGX Distribution multiplied by combined approximation of Visibility and Fresnel
    // BRDFspec = (D * V * F) / 4.0
    // D = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2
    // V * F = 1.0 / ( LoH^2 * (roughness + 0.5) )
    // See "Optimizing PBR for Mobile" from Siggraph 2015 moving mobile graphics course
    // https://community.arm.com/events/1155

    // Final BRDFspec = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2 * (LoH^2 * (roughness + 0.5) * 4.0)
    // We further optimize a few light invariant terms
    // brdfData.normalizationTerm = (roughness + 0.5) * 4.0 rewritten as roughness * 4.0 + 2.0 to a fit a MAD.
    float d = NoH * NoH * (roughness2 - 1) + 1.00001f;

    half LoH2 = LoH * LoH;
    half specularTerm = roughness2 / ((d * d) * max(0.1h, LoH2) * normalizationTerm);

    // On platforms where half actually means something, the denominator has a risk of overflow
    // clamp below was added specifically to "fix" that, but dx compiler (we convert bytecode to metal/gles)
    // sees that specularTerm have only non-negative terms, so it skips max(0,..) in clamp (leaving only min(100,...))
#if defined (SHADER_API_MOBILE) || defined (SHADER_API_SWITCH)
    specularTerm = specularTerm - HALF_MIN;
    specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
#endif
    
    half NdotL = saturate(dot(N, L));
    return (albedo + specularTerm * f0) * NdotL * lightColor;
}

//#define LIGHTMAP_SHADOW_MIXING

void PbrLighting_float(float3 Position, float3 Normal, float3 ViewDirection, float3 Albedo, float Smoothness, float Metallic, float3 BakedGI, out float3 Color)
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
    Color = CustomLightHandling(mainLight.color * mainLight.shadowAttenuation, mainLight.direction, Normal, Albedo, ViewDirection, roughness, specularColor);
    
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