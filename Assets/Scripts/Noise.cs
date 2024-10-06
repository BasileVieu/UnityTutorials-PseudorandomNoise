using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public static partial class Noise
{
    public interface INoise
    {
        float4 GetNoise4(float4x3 _positions, SmallXXHash4 _hash, int _frequency);
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Job<N> : IJobFor where N : struct, INoise
    {
        [ReadOnly] public NativeArray<float3x4> positions;

        [WriteOnly] public NativeArray<float4> noise;

        public Settings settings;

        public float3x4 domainTRS;

        public void Execute(int _i)
        {
            float4x3 position = domainTRS.TransformVectors(transpose(positions[_i]));
            
            SmallXXHash4 hash = SmallXXHash4.Seed(settings.seed);

            int frequency = settings.frequency;

            var amplitude = 1.0f;
            var amplitudeSum = 0.0f;
            
            float4 sum = 0.0f;

            for (var o = 0; o < settings.octaves; o++)
            {
                sum += amplitude * default(N).GetNoise4(position, hash + o, frequency);
                amplitudeSum += amplitude;
                frequency *= settings.lacunarity;
                amplitude *= settings.persistence;
            }
            
            noise[_i] = sum / amplitudeSum;
        }

        public static JobHandle ScheduleParallel(NativeArray<float3x4> _positions, NativeArray<float4> _noise,
                                                 Settings _settings, SpaceTRS _trs, int _resolution, JobHandle _dependency) =>
                new Job<N>
                {
                        positions = _positions,
                        noise = _noise,
                        settings = _settings,
                        domainTRS = _trs.Matrix
                }.ScheduleParallel(_positions.Length, _resolution, _dependency);
    }

    public delegate JobHandle ScheduleDelegate(NativeArray<float3x4> _positions, NativeArray<float4> _noise, Settings _settings,
                                               SpaceTRS _trs, int _resolution, JobHandle _dependency);
}