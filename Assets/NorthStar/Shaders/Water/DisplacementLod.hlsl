// Copyright (c) Meta Platforms, Inc. and affiliates.

#ifndef OCEAN_DISLACEMENT_LOD_INCLUDED
#define OCEAN_DISLACEMENT_LOD_INCLUDED

UNITY_INSTANCING_BUFFER_START(OceanData)
UNITY_DEFINE_INSTANCED_PROP(float, _OceanInstanceScales)
UNITY_INSTANCING_BUFFER_END(OceanData)

void DisplacementLod_float(out float lod)
{
    lod = UNITY_ACCESS_INSTANCED_PROP(OceanData, _OceanInstanceScales);
}

void DisplacementLod_half(out half lod)
{
    lod =  UNITY_ACCESS_INSTANCED_PROP(OceanData, _OceanInstanceScales);
}

#endif