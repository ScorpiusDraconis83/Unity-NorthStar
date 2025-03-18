#ifndef DIRECTION_TO_UV_PROJECTION_UNITY
#define DIRECTION_TO_UV_PROJECTION_UNITY

void DirectionToCubemapUV_float(
    float3 Direction,
    out float2 UV)
{
    float3 dir = normalize(Direction);
    float3 absDir = abs(dir);
    float maxAxis = max(max(absDir.x, absDir.y), absDir.z);
    float2 scaledUV = float2(0, 0);
    float faceIndex = 0;
    const float uvScale = 0.997;
    
    if (maxAxis == absDir.x && dir.x > 0)
    {
        scaledUV = float2(dir.z / dir.x, -dir.y / dir.x) * uvScale;
        faceIndex = 0;
    }
    else if (maxAxis == absDir.x)
    {
        scaledUV = float2(dir.z / dir.x, dir.y / dir.x) * uvScale;
        faceIndex = 1;
    }
    else if (maxAxis == absDir.y && dir.y > 0)
    {
        scaledUV = float2(dir.x / dir.y, -dir.z / dir.y) * uvScale;
        faceIndex = 3;
    }
    else if (maxAxis == absDir.y)
    {
        scaledUV = float2(-dir.x / dir.y, -dir.z / dir.y) * uvScale;
        faceIndex = 2;
    }
    else if (maxAxis == absDir.z && dir.z > 0)
    {
        scaledUV = float2(-dir.x / dir.z, -dir.y / dir.z) * uvScale;
        faceIndex = 5;
    }
    else
    {
        scaledUV = float2(-dir.x / dir.z, dir.y / dir.z) * uvScale;
        faceIndex = 4;
    }
    
    UV.x = (scaledUV.x * 0.5 + 0.5) / 6.0 + faceIndex / 6.0;
    UV.y = scaledUV.y * 0.5 + 0.5;
}
#endif