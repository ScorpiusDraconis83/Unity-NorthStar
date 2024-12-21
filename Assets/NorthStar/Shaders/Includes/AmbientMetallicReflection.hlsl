// Copyright (c) Meta Platforms, Inc. and affiliates.

void AmbientMetallicReflection_float(float3 Normal, float3 ViewDirection, float Smoothness, float3 SpecularColor, out float3 Reflection)
{
    #if SHADERGRAPH_PREVIEW
        Reflection = 0.0;
    #else
        float perceptualRoughness = 1.0 - Smoothness;
        float3 R = reflect(-ViewDirection, Normal);
        float mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
        float4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, R, mip);
        float3 irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);

        float roughness = Sq(perceptualRoughness);


        float3 fresnel = EnvBRDFApprox(SpecularColor , roughness, dot(Normal, ViewDirection));
        float3 reflectionColor = irradiance * fresnel;
        Reflection = reflectionColor;
    #endif
}