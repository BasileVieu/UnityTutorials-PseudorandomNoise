using Unity.Mathematics;
using static Unity.Mathematics.math;
using float4 = Unity.Mathematics.float4;

public static partial class Noise
{
    public struct Simplex1D<G> : INoise where G : struct, IGradient
    {
        public float4 GetNoise4(float4x3 _positions, SmallXXHash4 _hash, int _frequency)
        {
            _positions *= _frequency;

            var x0 = (int4)floor(_positions.c0);
            int4 x1 = x0 + 1;
            
            return default(G).EvaluateCombined(Kernel(_hash.Eat(x0), x0, _positions)
                                               + Kernel(_hash.Eat(x1), x1, _positions));
        }

        static float4 Kernel(SmallXXHash4 _hash, float4 _lx, float4x3 _positions)
        {
            float4 x = _positions.c0 - _lx;
            float4 f = 1.0f - x * x;
            f = f * f * f;

            return f * default(G).Evaluate(_hash, x);
        }
    }
    
    public struct Simplex2D<G> : INoise where G : struct, IGradient
    {
        public float4 GetNoise4(float4x3 _positions, SmallXXHash4 _hash, int _frequency)
        {
            _positions *= _frequency * (1.0f / sqrt(3.0f));

            float4 skew = (_positions.c0 + _positions.c2) * ((sqrt(3.0f) - 1.0f) / 2.0f);
            
            float4 sx = _positions.c0 + skew;
            float4 sz = _positions.c2 + skew;

            var x0 = (int4)floor(sx);
            int4 x1 = x0 + 1;

            var z0 = (int4)floor(sz);
            int4 z1 = z0 + 1;

            bool4 xGz = sx - x0 > sz - z0;

            int4 xC = select(x0, x1, xGz);
            int4 zC = select(z1, z0, xGz);

            SmallXXHash4 h0 = _hash.Eat(x0);
            SmallXXHash4 h1 = _hash.Eat(x1);
            SmallXXHash4 hC = SmallXXHash4.Select(h0, h1, xGz);
            
            return default(G).EvaluateCombined(Kernel(h0.Eat(z0), x0, z0, _positions)
                                               + Kernel(h1.Eat(z1), x1, z1, _positions)
                                               + Kernel(hC.Eat(zC), xC, zC, _positions));
        }

        static float4 Kernel(SmallXXHash4 _hash, float4 _lx, float4 _lz, float4x3 _positions)
        {
            float4 unskew = (_lx + _lz) * ((3.0f - sqrt(3.0f)) / 6.0f);
            float4 x = _positions.c0 - _lx + unskew;
            float4 z = _positions.c2 - _lz + unskew;
            float4 f = 0.5f - x * x - z * z;
            f = f * f * f * 8.0f;

            return max(0.0f, f) * default(G).Evaluate(_hash, x, z);
        }
    }
    
    public struct Simplex3D<G> : INoise where G : struct, IGradient
    {
        public float4 GetNoise4(float4x3 _positions, SmallXXHash4 _hash, int _frequency)
        {
            _positions *= _frequency * 0.6f;

            float4 skew = (_positions.c0 + _positions.c1 + _positions.c2) * (1.0f / 3.0f);
            
            float4 sx = _positions.c0 + skew;
            float4 sy = _positions.c1 + skew;
            float4 sz = _positions.c2 + skew;

            var x0 = (int4)floor(sx);
            int4 x1 = x0 + 1;

            var y0 = (int4)floor(sy);
            int4 y1 = y0 + 1;

            var z0 = (int4)floor(sz);
            int4 z1 = z0 + 1;

            bool4 xGy = sx - x0 > sy - y0;
            bool4 xGz = sx - x0 > sz - z0;
            bool4 yGz = sy - y0 > sz - z0;

            bool4 xA = xGy & xGz;
            bool4 xB = xGy | (xGz & yGz);
            bool4 yA = !xGy & yGz;
            bool4 yB = !xGy | (xGz & yGz);
            bool4 zA = (xGy & !xGz) | (!xGy & !yGz);
            bool4 zB = !(xGz & yGz);

            int4 xCA = select(x0, x1, xA);
            int4 xCB = select(x0, x1, xB);
            int4 yCA = select(y0, y1, yA);
            int4 yCB = select(y0, y1, yB);
            int4 zCA = select(z0, z1, zA);
            int4 zCB = select(z0, z1, zB);

            SmallXXHash4 h0 = _hash.Eat(x0);
            SmallXXHash4 h1 = _hash.Eat(x1);
            SmallXXHash4 hA = SmallXXHash4.Select(h0, h1, xA);
            SmallXXHash4 hB = SmallXXHash4.Select(h0, h1, xB);
            
            return default(G).EvaluateCombined(Kernel(h0.Eat(y0).Eat(z0), x0, y0, z0, _positions)
                                               + Kernel(h1.Eat(y1).Eat(z1), x1, y1, z1, _positions)
                                               + Kernel(hA.Eat(yCA).Eat(zCA), xCA, yCA, zCA, _positions)
                                               + Kernel(hB.Eat(yCB).Eat(zCB), xCB, yCB, zCB, _positions));
        }

        static float4 Kernel(SmallXXHash4 _hash, float4 _lx, float4 _ly, float4 _lz, float4x3 _positions)
        {
            float4 unskew = (_lx + _ly + _lz) * (1.0f / 6.0f);
            float4 x = _positions.c0 - _lx + unskew;
            float4 y = _positions.c1 - _ly + unskew;
            float4 z = _positions.c2 - _lz + unskew;
            float4 f = 0.5f - x * x - y * y - z * z;
            f = f * f * f * 8.0f;

            return max(0.0f, f) * default(G).Evaluate(_hash, x, y, z);
        }
    }
}