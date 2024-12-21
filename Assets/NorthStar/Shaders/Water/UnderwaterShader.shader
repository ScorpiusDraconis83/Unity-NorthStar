// Copyright (c) Meta Platforms, Inc. and affiliates.

Shader "Unlit/UnderwaterShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #include "UnderwaterShader.hlsl"
            ENDHLSL
        }
    }
}
