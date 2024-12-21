// Copyright (c) Meta Platforms, Inc. and affiliates.

Shader "Hidden/Ocean Normal Blit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float4 frag (v2f_img i) : SV_Target
            {
                float4 packedNormal = tex2D(_MainTex, i.uv);

                float3 normal;
                normal.xy = 2.0 * packedNormal.rg - 1.0;
                normal.z = sqrt(saturate(1.0 - dot(normal.xy, normal.xy)));
                return float4(0.5 * normal + 0.5, 1.0);
            }

            ENDCG
        }
    }
}
