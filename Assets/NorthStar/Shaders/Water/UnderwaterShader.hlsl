// Copyright (c) Meta Platforms, Inc. and affiliates.

struct FragmentInput
{
    float4 position : SV_Position;
};

matrix unity_ObjectToWorld, unity_MatrixVP;

FragmentInput Vertex(float3 position : POSITION)
{
    float3 worldPosition = mul(unity_ObjectToWorld, float4(position, 1)).xyz;
    
    FragmentInput output;
    output.position = mul(unity_MatrixVP, float4(worldPosition, 1));
    return output;
}

float3 Fragment(FragmentInput input) : SV_Target
{
    return 1;
}