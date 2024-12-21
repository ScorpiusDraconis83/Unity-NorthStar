// Copyright (c) Meta Platforms, Inc. and affiliates.

sampler2D _OceanHeight, _OceanDisplacement;
float4 _OceanHeight_TexelSize, _OceanDisplacement_TexelSize;
float _OceanRcpScale;

// TODO: Move into a common inlcude if we use this in more than one place
float4 VertexFullscreenTriangle(uint id : SV_VertexID, out float2 uv : TEXCOORD) : SV_Position
{
	uv = (id <<  uint2(1, 0)) & 2;
	float4 result = float3(2.0 * uv - 1.0, 1.0).xyzz;
	uv.y = 1.0 - uv.y;
	return result;
}

float4 Fragment(float4 position : SV_Position, float2 uv : TEXCOORD) : SV_Target
{
	float leftHeight = tex2D(_OceanHeight, uv + _OceanHeight_TexelSize.xy * float2(-1, 0)).r;
	float rightHeight = tex2D(_OceanHeight, uv + _OceanHeight_TexelSize.xy * float2(1, 0)).r;
	float backHeight = tex2D(_OceanHeight, uv + _OceanHeight_TexelSize.xy * float2(0, -1)).r;
	float forwardHeight = tex2D(_OceanHeight, uv + _OceanHeight_TexelSize.xy * float2(0, 1)).r;
    
	float2 delta = _OceanHeight_TexelSize.zw * _OceanRcpScale;

	float xSlope = leftHeight - rightHeight;
	float zSlope = backHeight - forwardHeight;

	 // Store foam (folding) in w
	float2 leftDisplacement = tex2D(_OceanDisplacement, uv + _OceanDisplacement_TexelSize.xy * float2(-1, 0)).rg;
	float2 rightDisplacement = tex2D(_OceanDisplacement, uv + _OceanDisplacement_TexelSize.xy * float2(1, 0)).rg;
	float2 backDisplacement = tex2D(_OceanDisplacement, uv + _OceanDisplacement_TexelSize.xy * float2(0, -1)).rg;
	float2 forwardDisplacement = tex2D(_OceanDisplacement, uv + _OceanDisplacement_TexelSize.xy * float2(0, 1)).rg;
	
	float2 dx = leftDisplacement - rightDisplacement;
	float2 dz = backDisplacement - forwardDisplacement;

	float2 jx = -dx * delta;
	float2 jz = -dz * delta;

	float jacobian = (1 + jx.x) * (1 + jz.y) - jx.y * jz.x;
	float4 result = float4(normalize(float3(xSlope * delta.x, 4, zSlope * delta.y)) * 0.5f + 0.5f, saturate(jacobian));
	
	return result;
}