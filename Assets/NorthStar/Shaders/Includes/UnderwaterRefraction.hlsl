// Copyright (c) Meta Platforms, Inc. and affiliates.

#ifndef UNDERWATER_REFRACTION_INCLUDED
#define UNDERWATER_REFRACTION_INCLUDED

void UnderwaterRefraction_float(float3 N, float3 V, float smoothness, bool isFrontFace, out float3 color)
{
    #if SHADERGRAPH_PREVIEW
	color = 0.0;
#else
    float perceptualRoughness = 1.0 - smoothness;
	float roughness = Sq(perceptualRoughness);
    float envBrdf;
    
    // Reflect or refract based on critical angle
    float3 R;
    if(isFrontFace)
    {
        R = reflect(-V, N);
        envBrdf = EnvBRDFApproxNonmetal(roughness, dot(N, V));
    }
    else
    {
        float eta = 2;
        float NdotI = dot(-N, -V);
        float k = 1.0 - eta * eta * (1.0 - NdotI * NdotI);
        if(k < 0.0)
        {
            envBrdf = EnvBRDFApproxNonmetal(roughness, dot(-N, V));
            R = reflect(-V, -N);
        }
        else
        {
            envBrdf = 1.0 - EnvBRDFApproxNonmetal(roughness, dot(-N, V));
            R = eta * -V - (eta * NdotI + sqrt(k)) * -N;
        }
    }
    
 	float mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
 	float4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, R, 0);
 	float3 irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
 
 	color = irradiance * envBrdf;
    
#endif
}
#endif