using Unity.Mathematics;
using static Unity.Mathematics.math;

public static partial class Noise
{
    public interface IVoronoiDistance
    {
        float4 GetDistance(float4 _x);
        
        float4 GetDistance(float4 _x, float4 _y);
        
        float4 GetDistance(float4 _x, float4 _y, float4 _z);

        float4x2 Finalize1D(float4x2 _minima);

        float4x2 Finalize2D(float4x2 _minima);

        float4x2 Finalize3D(float4x2 _minima);
    }

    public struct Worley : IVoronoiDistance
    {
        public float4 GetDistance(float4 _x) => abs(_x);

        public float4 GetDistance(float4 _x, float4 _y) => _x * _x + _y * _y;

        public float4 GetDistance(float4 _x, float4 _y, float4 _z) => _x * _x + _y * _y + _z * _z;

        public float4x2 Finalize1D(float4x2 _minima) => _minima;

        public float4x2 Finalize2D(float4x2 _minima)
        {
            _minima.c0 = sqrt(min(_minima.c0, 1.0f));
            _minima.c1 = sqrt(min(_minima.c1, 1.0f));

            return _minima;
        }

        public float4x2 Finalize3D(float4x2 _minima) => Finalize2D(_minima);
    }

    public struct Chebyshev : IVoronoiDistance
    {
        public float4 GetDistance(float4 _x) => abs(_x);

        public float4 GetDistance(float4 _x, float4 _y) => max(abs(_x), abs(_y));

        public float4 GetDistance(float4 _x, float4 _y, float4 _z) => max(max(abs(_x), abs(_y)), abs(_z));

        public float4x2 Finalize1D(float4x2 _minima) => _minima;

        public float4x2 Finalize2D(float4x2 _minima) => _minima;

        public float4x2 Finalize3D(float4x2 _minima) => _minima;
    }
}