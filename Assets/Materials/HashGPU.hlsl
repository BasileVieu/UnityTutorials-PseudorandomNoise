#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    StructuredBuffer<uint> _Hashes;
    StructuredBuffer<float3> _Positions;
    StructuredBuffer<float3> _Normals;
#endif

float4 _Config;

void ConfigureProcedural()
{
    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        unity_ObjectToWorld = 0.0f;
        unity_ObjectToWorld._m03_m13_m23_m33 = float4(_Positions[unity_InstanceID], 1.0f);
        unity_ObjectToWorld._m03_m13_m23 += (_Config.z * ((1.0f / 255.0f) * (_Hashes[unity_InstanceID] >> 24) - 0.5f)) * _Normals[unity_InstanceID];
        unity_ObjectToWorld._m00_m11_m22 = _Config.y;
    #endif
}

float3 GetHashColor()
{
    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        uint hash = _Hashes[unity_InstanceID];
    
        return (1.0f / 255.0f) * float3(hash & 255, (hash >> 8) & 255, (hash >> 16) & 255);
    #else
        return 1.0f;
    #endif
}

void ShaderGraphFunction_float(float3 In, out float3 Out, out float3 Color)
{
    Out = In;
    Color = GetHashColor();
}

void ShaderGraphFunction_half(half3 In, out half3 Out, out half3 Color)
{
    Out = In;
    Color = GetHashColor();
}