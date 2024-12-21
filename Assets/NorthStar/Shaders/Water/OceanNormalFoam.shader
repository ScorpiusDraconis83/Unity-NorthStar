// Copyright (c) Meta Platforms, Inc. and affiliates.

Shader "Hidden/Ocean Normal Foam"
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
            HLSLPROGRAM

            #pragma vertex VertexFullscreenTriangle
            #pragma fragment Fragment

            #include "OceanNormalFoam.hlsl"
            ENDHLSL
        }
    }
}