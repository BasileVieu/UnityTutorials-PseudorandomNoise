using Unity.Mathematics;
using static Unity.Mathematics.math;

public static class MathExtensions
{
    public static float4x3 TransformVectors(this float3x4 _trs, float4x3 _p, float _w = 1f) =>
            float4x3(_trs.c0.x * _p.c0 + _trs.c1.x * _p.c1 + _trs.c2.x * _p.c2 + _trs.c3.x * _w,
                     _trs.c0.y * _p.c0 + _trs.c1.y * _p.c1 + _trs.c2.y * _p.c2 + _trs.c3.y * _w,
                     _trs.c0.z * _p.c0 + _trs.c1.z * _p.c1 + _trs.c2.z * _p.c2 + _trs.c3.z * _w);

    public static float3x4 Get3x4(this float4x4 _m) =>
            float3x4(_m.c0.xyz, _m.c1.xyz, _m.c2.xyz, _m.c3.xyz);
}