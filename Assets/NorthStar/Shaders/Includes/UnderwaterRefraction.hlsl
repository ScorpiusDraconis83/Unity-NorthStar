// Copyright (c) Meta Platforms, Inc. and affiliates.

#ifndef UNDERWATER_REFRACTION_INCLUDED
#define UNDERWATER_REFRACTION_INCLUDED

void UnderwaterRefraction_half(half3 N, half3 V, half smoothness, bool isFrontFace, out half3 color)
{
    #if SHADERGRAPH_PREVIEW
	color = 0.0;
#else
    half perceptualRoughness = 1.0 - smoothness;
    half roughness = Sq(perceptualRoughness);
    half envBrdf;
    
    // Reflect or refract based on critical angle
    half3 R;
    if(isFrontFace)
    {
        R = reflect(-V, N);
        envBrdf = EnvBRDFApproxNonmetal(roughness, dot(N, V));
    }
    else
    {
        half eta = 2;
        half NdotI = dot(-N, -V);
        half k = 1.0 - eta * eta * (1.0 - NdotI * NdotI);
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
    
    half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
    half4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, R, 0);
    half3 irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
 
 	color = irradiance * envBrdf;
    
#endif
}
void UnderwaterRefraction_float(half3 N, half3 V, half smoothness, bool isFrontFace, out half3 color)
{
    UnderwaterRefraction_half(N, V, smoothness, isFrontFace, color);
}

#endif