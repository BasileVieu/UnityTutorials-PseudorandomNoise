using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

public class HashVisualization : Visualization
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct HashJob : IJobFor
    {
        [WriteOnly] public NativeArray<uint4> hashes;

        [ReadOnly] public NativeArray<float3x4> positions;

        public SmallXXHash4 hash;

        public float3x4 domainTRS;

        public void Execute(int _i)
        {
            float4x3 p = domainTRS.TransformVectors(transpose(positions[_i]));

            int4 u = (int4) floor(p.c0);
            int4 v = (int4) floor(p.c1);
            int4 w = (int4) floor(p.c2);

            hashes[_i] = hash.Eat(u).Eat(v).Eat(w);
        }
    }

    private static int hashesId = Shader.PropertyToID("_Hashes");

    [SerializeField] private int seed;

    [SerializeField] private SpaceTRS domain = new SpaceTRS
    {
            scale = 8.0f
    };

    private NativeArray<uint4> hashes;

    private ComputeBuffer hashesBuffer;

    protected override void EnableVisualization(int _dataLength, MaterialPropertyBlock _propertyBlock)
    {
        hashes = new NativeArray<uint4>(_dataLength, Allocator.Persistent);

        hashesBuffer = new ComputeBuffer(_dataLength * 4, 4);

        _propertyBlock.SetBuffer(hashesId, hashesBuffer);
    }

    protected override void DisableVisualization()
    {
        hashes.Dispose();

        hashesBuffer.Release();

        hashesBuffer = null;
    }

    protected override void UpdateVisualization(NativeArray<float3x4> _positions, int _resolution, JobHandle _handle)
    {
        new HashJob
        {
                positions = _positions,
                hashes = hashes,
                hash = SmallXXHash.Seed(seed),
                domainTRS = domain.Matrix
        }.ScheduleParallel(hashes.Length, _resolution, _handle).Complete();

        hashesBuffer.SetData(hashes);
    }
}