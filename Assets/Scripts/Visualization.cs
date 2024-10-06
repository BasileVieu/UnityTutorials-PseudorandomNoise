using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

public abstract class Visualization : MonoBehaviour
{
    public enum Shape
    {
        Plane,
        Sphere,
        Torus
    }

    private static Shapes.ScheduleDelegate[] shapeJobs =
    {
            Shapes.Job<Shapes.Plane>.ScheduleParallel,
            Shapes.Job<Shapes.Sphere>.ScheduleParallel,
            Shapes.Job<Shapes.Torus>.ScheduleParallel
    };

    private static int positionsId = Shader.PropertyToID("_Positions");
    private static int normalsId = Shader.PropertyToID("_Normals");
    private static int configId = Shader.PropertyToID("_Config");

    [SerializeField] private Mesh instanceMesh;

    [SerializeField] private Material material;

    [SerializeField] private Shape shape;

    [SerializeField] [Range(0.1f, 10.0f)] private float instanceScale = 2.0f;

    [SerializeField] [Range(1, 512)] private int resolution = 16;

    [SerializeField] [Range(-0.5f, 0.5f)] private float displacement = 0.1f;

    private NativeArray<float3x4> positions;
    private NativeArray<float3x4> normals;

    private ComputeBuffer positionsBuffer;
    private ComputeBuffer normalsBuffer;

    private MaterialPropertyBlock propertyBlock;

    private Bounds bounds;

    private bool isDirty;

    void OnEnable()
    {
        isDirty = true;

        int length = resolution * resolution;
        length = length / 4 + (length & 1);

        positions = new NativeArray<float3x4>(length, Allocator.Persistent);
        normals = new NativeArray<float3x4>(length, Allocator.Persistent);
        
        positionsBuffer = new ComputeBuffer(length * 4, 3 * 4);
        normalsBuffer = new ComputeBuffer(length * 4, 3 * 4);

        propertyBlock ??= new MaterialPropertyBlock();

        EnableVisualization(length, propertyBlock);
        
        propertyBlock.SetBuffer(positionsId, positionsBuffer);
        propertyBlock.SetBuffer(normalsId, normalsBuffer);
        propertyBlock.SetVector(configId, new Vector4(resolution, instanceScale / resolution, displacement));
    }

    void OnDisable()
    {
        positions.Dispose();
        normals.Dispose();
        
        positionsBuffer.Release();
        normalsBuffer.Release();
        
        positionsBuffer = null;
        normalsBuffer = null;

        DisableVisualization();
    }

    void OnValidate()
    {
        if (positionsBuffer != null
            && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    void Update()
    {
        if (isDirty
            || transform.hasChanged)
        {
            isDirty = false;
            transform.hasChanged = false;

            UpdateVisualization(positions, resolution, shapeJobs[(int)shape](positions, normals, resolution, transform.localToWorldMatrix, default));

            positionsBuffer.SetData(positions.Reinterpret<float3>(3 * 4 * 4));
            normalsBuffer.SetData(normals.Reinterpret<float>(3 * 4 * 4));

            bounds = new Bounds(transform.position, float3(2.0f * cmax(abs(transform.lossyScale)) + displacement));
        }

        Graphics.DrawMeshInstancedProcedural(instanceMesh, 0, material, bounds, resolution * resolution, propertyBlock);
    }

    protected abstract void EnableVisualization(int _dataLength, MaterialPropertyBlock _propertyBlock);

    protected abstract void DisableVisualization();

    protected abstract void UpdateVisualization(NativeArray<float3x4> _positions, int _resolution, JobHandle _handle);
}