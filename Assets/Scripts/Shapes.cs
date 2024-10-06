using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public static class Shapes
{
    public struct Point4
    {
        public float4x3 positions;
        public float4x3 normals;
    }

    public interface IShape
    {
        Point4 GetPoint4(int _i, float _resolution, float _invResolution);
    }

    public struct Plane : IShape
    {
        public Point4 GetPoint4(int _i, float _resolution, float _invResolution)
        {
            float4x2 uv = IndexTo4UV(_i, _resolution, _invResolution);

            return new Point4
            {
                    positions = float4x3(uv.c0 - 0.5f, 0.0f, uv.c1 - 0.5f),
                    normals = float4x3(0.0f, 1.0f, 0.0f)
            };
        }
    }

    public struct Sphere : IShape
    {
        public Point4 GetPoint4(int _i, float _resolution, float _invResolution)
        {
            float4x2 uv = IndexTo4UV(_i, _resolution, _invResolution);

            Point4 p;
            p.positions.c0 = uv.c0 - 0.5f;
            p.positions.c1 = uv.c1 - 0.5f;
            p.positions.c2 = 0.5f - abs(p.positions.c0) - abs(p.positions.c1);

            float4 offset = max(-p.positions.c2, 0.0f);

            p.positions.c0 += select(-offset, offset, p.positions.c0 < 0.0f);
            p.positions.c1 += select(-offset, offset, p.positions.c1 < 0.0f);

            float4 scale = 0.5f * rsqrt(p.positions.c0 * p.positions.c0 +
                                        p.positions.c1 * p.positions.c1 +
                                        p.positions.c2 * p.positions.c2);

            p.positions.c0 *= scale;
            p.positions.c1 *= scale;
            p.positions.c2 *= scale;
            
            p.normals = p.positions;

            return p;
        }
    }

    public struct Torus : IShape
    {
        public Point4 GetPoint4(int _i, float _resolution, float _invResolution)
        {
            float4x2 uv = IndexTo4UV(_i, _resolution, _invResolution);

            float r1 = 0.375f;
            float r2 = 0.125f;

            float4 s = r1 + r2 * cos(2.0f * PI * uv.c1);

            Point4 p;
            p.positions.c0 = s * sin(2.0f * PI * uv.c0);
            p.positions.c1 = r2 * sin(2.0f * PI * uv.c1);
            p.positions.c2 = s * cos(2.0f * PI * uv.c0);
            p.normals = p.positions;
            p.normals.c0 -= r1 * sin(2.0f * PI * uv.c0);
            p.normals.c2 -= r1 * cos(2.0f * PI * uv.c0);

            return p;
        }
    }

    public delegate JobHandle ScheduleDelegate(NativeArray<float3x4> _positions, NativeArray<float3x4> _normals, int _resolution, float4x4 _trs, JobHandle _dependency);
    
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Job<S> : IJobFor where S : struct, IShape
    {
        [WriteOnly] private NativeArray<float3x4> positions;
        [WriteOnly] private NativeArray<float3x4> normals;

        public float resolution;
        public float invResolution;

        public float3x4 positionTRS;
        public float3x4 normalTRS;

        public void Execute(int _i)
        {
            Point4 p = default(S).GetPoint4(_i, resolution, invResolution);

            positions[_i] = transpose(positionTRS.TransformVectors(p.positions));

            float3x4 n = transpose(normalTRS.TransformVectors(p.normals, 0.0f));

            normals[_i] = float3x4(normalize(n.c0), normalize(n.c1), normalize(n.c2), normalize(n.c3));
        }

        public static JobHandle ScheduleParallel(NativeArray<float3x4> _positions, NativeArray<float3x4> _normals,
                                                 int _resolution, float4x4 _trs, JobHandle _dependency) =>
                new Job<S>
                {
                        positions = _positions,
                        normals = _normals,
                        resolution = _resolution,
                        invResolution = 1.0f / _resolution,
                        positionTRS = _trs.Get3x4(),
                        normalTRS = transpose(inverse(_trs)).Get3x4()
                }.ScheduleParallel(_positions.Length, _resolution, _dependency);
    }

    public static float4x2 IndexTo4UV(int _i, float _resolution, float _invResolution)
    {
        float4x2 uv;

        float4 i4 = 4.0f * _i + float4(0.0f, 1.0f, 2.0f, 3.0f);

        uv.c1 = floor(_invResolution * i4 + 0.00001f);
        uv.c0 = _invResolution * (i4 - _resolution * uv.c1 + 0.5f);
        uv.c1 = _invResolution * (uv.c1 + 0.5f);

        return uv;
    }
}