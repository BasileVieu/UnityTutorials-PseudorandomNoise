using Unity.Mathematics;
using static Unity.Mathematics.math;
using float4 = Unity.Mathematics.float4;

public static partial class Noise
{
    public interface IGradient
    {
        float4 Evaluate(SmallXXHash4 _hash, float4 _x);
        
        float4 Evaluate(SmallXXHash4 _hash, float4 _x, float4 _y);
        
        float4 Evaluate(SmallXXHash4 _hash, float4 _x, float4 _y, float4 _z);

        float4 EvaluateCombined(float4 _value);
    }

    public struct Value : IGradient
    {
        public float4 Evaluate(SmallXXHash4 _hash, float4 _x) => _hash.Floats01A * 2.0f - 1.0f;
        
        public float4 Evaluate(SmallXXHash4 _hash, float4 _x, float4 _y) => _hash.Floats01A * 2.0f - 1.0f;
        
        public float4 Evaluate(SmallXXHash4 _hash, float4 _x, float4 _y, float4 _z) => _hash.Floats01A * 2.0f - 1.0f;
        
        public float4 EvaluateCombined(float4 _value) => _value;
    }

    public struct Perlin : IGradient
    {
        public float4 Evaluate(SmallXXHash4 _hash, float4 _x) => BaseGradients.Line(_hash, _x);

        public float4 Evaluate(SmallXXHash4 _hash, float4 _x, float4 _y) => BaseGradients.Square(_hash, _x, _y) * (2.0f / 0.53528f);

        public float4 Evaluate(SmallXXHash4 _hash, float4 _x, float4 _y, float4 _z) => BaseGradients.Octahedron(_hash, _x, _y, _z) * (1.0f / 0.56290f);

        public float4 EvaluateCombined(float4 _value) => _value;
    }

    public struct Simplex : IGradient
    {
        public float4 Evaluate(SmallXXHash4 _hash, float4 _x) => BaseGradients.Line(_hash, _x) * (32.0f / 27.0f);
        
        public float4 Evaluate(SmallXXHash4 _hash, float4 _x, float4 _y) => BaseGradients.Circle(_hash, _x, _y) * (5.832f / sqrt(2.0f));
        
        public float4 Evaluate(SmallXXHash4 _hash, float4 _x, float4 _y, float4 _z) => BaseGradients.Sphere(_hash, _x, _y, _z) * (1024.0f / (125.0f * sqrt(3.0f)));

        public float4 EvaluateCombined(float4 _value) => _value;
    }

    public static class BaseGradients
    {
        static float4x2 SquareVectors(SmallXXHash4 _hash)
        {
            float4x2 v;
            v.c0 = _hash.Floats01A * 2.0f - 1.0f;
            v.c1 = 0.5f - abs(v.c0);
            v.c0 -= floor(v.c0 + 0.5f);

            return v;
        }

        static float4x3 OctahedronVectors(SmallXXHash4 _hash)
        {
            float4x3 g;
            g.c0 = _hash.Floats01A * 2.0f - 1.0f;
            g.c1 = _hash.Floats01D * 2.0f - 1.0f;
            g.c2 = 1.0f - abs(g.c0) - abs(g.c1);

            float4 offset = max(-g.c2, 0.0f);

            g.c0 += select(-offset, offset, g.c0 < 0.0f);
            g.c1 += select(-offset, offset, g.c1 < 0.0f);

            return g;
        }

        public static float4 Line(SmallXXHash4 _hash, float4 _x) => (1.0f + _hash.Floats01A) * select(-_x, _x, ((uint4)_hash & 1 << 8) == 0);

        public static float4 Square(SmallXXHash4 _hash, float4 _x, float4 _y)
        {
            float4x2 v = SquareVectors(_hash);

            return v.c0 * _x + v.c1 * _y;
        }

        public static float4 Circle(SmallXXHash4 _hash, float4 _x, float4 _y)
        {
            float4x2 v = SquareVectors(_hash);

            return (v.c0 * _x + v.c1 * _y) * rsqrt(v.c0 * v.c0 + v.c1 * v.c1);
        }

        public static float4 Octahedron(SmallXXHash4 _hash, float4 _x, float4 _y, float4 _z)
        {
            float4x3 v = OctahedronVectors(_hash);

            return v.c0 * _x + v.c1 * _y + v.c2 * _z;
        }

        public static float4 Sphere(SmallXXHash4 _hash, float4 _x, float4 _y, float4 _z)
        {
            float4x3 v = OctahedronVectors(_hash);

            return (v.c0 * _x + v.c1 * _y + v.c2 * _z) * rsqrt(v.c0 * v.c0 + v.c1 * v.c1 + v.c2 * v.c2);
        }
    }

    public struct Turbulence<G> : IGradient where G : IGradient
    {
        public float4 Evaluate(SmallXXHash4 _hash, float4 _x) => default(G).Evaluate(_hash, _x);

        public float4 Evaluate(SmallXXHash4 _hash, float4 _x, float4 _y) => default(G).Evaluate(_hash, _x, _y);

        public float4 Evaluate(SmallXXHash4 _hash, float4 _x, float4 _y, float4 _z) => default(G).Evaluate(_hash, _x, _y, _z);

        public float4 EvaluateCombined(float4 _value) => abs(default(G).EvaluateCombined(_value));
    }
}