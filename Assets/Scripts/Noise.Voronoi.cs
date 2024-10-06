using Unity.Mathematics;
using static Unity.Mathematics.math;

public static partial class Noise
{
    static float4x2 UpdateVoronoiMinima(float4x2 _minima, float4 _distances)
    {
        bool4 newMinimum = _distances < _minima.c0;

        _minima.c1 = select(select(_minima.c1, _distances, _distances < _minima.c1), _minima.c0, newMinimum);
        _minima.c0 = select(_minima.c0, _distances, newMinimum);

        return _minima;
    }

    public struct Voronoi1D<L, D, F> : INoise where L : struct, ILattice where D : struct, IVoronoiDistance where F : struct, IVoronoiFunction
    {
        public float4 GetNoise4(float4x3 _positions, SmallXXHash4 _hash, int _frequency)
        {
            var l = default(L);
            var d = default(D);
            
            LatticeSpan4 x = l.GetLatticeSpan4(_positions.c0, _frequency);

            float4x2 minima = 2.0f;

            for (int u = -1; u <= 1; u++)
            {
                SmallXXHash4 h = _hash.Eat(l.ValidateSingleStep(x.p0 + u, _frequency));

                minima = UpdateVoronoiMinima(minima, d.GetDistance(h.Floats01A + u - x.g0));
            }

            return default(F).Evaluate(d.Finalize1D(minima));
        }
    }
    
    public struct Voronoi2D<L, D, F> : INoise where L : struct, ILattice where D : struct, IVoronoiDistance where F : struct, IVoronoiFunction
    {
        public float4 GetNoise4(float4x3 _positions, SmallXXHash4 _hash, int _frequency)
        {
            var l = default(L);
            var d = default(D);
            
            LatticeSpan4 x = l.GetLatticeSpan4(_positions.c0, _frequency);
            LatticeSpan4 z = l.GetLatticeSpan4(_positions.c2, _frequency);

            float4x2 minima = 2.0f;

            for (int u = -1; u <= 1; u++)
            {
                SmallXXHash4 hx = _hash.Eat(l.ValidateSingleStep(x.p0 + u, _frequency));

                float4 xOffset = u - x.g0;

                for (int v = -1; v <= 1; v++)
                {
                    SmallXXHash4 h = hx.Eat(l.ValidateSingleStep(z.p0 + v, _frequency));

                    float4 zOffset = v - z.g0;

                    minima = UpdateVoronoiMinima(minima, d.GetDistance(h.Floats01A + xOffset, h.Floats01B + zOffset));

                    minima = UpdateVoronoiMinima(minima, d.GetDistance(h.Floats01C + xOffset, h.Floats01D + zOffset));
                }
            }

            return default(F).Evaluate(d.Finalize2D(minima));
        }
    }
    
    public struct Voronoi3D<L, D, F> : INoise where L : struct, ILattice where D : struct, IVoronoiDistance where F : struct, IVoronoiFunction
    {
        public float4 GetNoise4(float4x3 _positions, SmallXXHash4 _hash, int _frequency)
        {
            var l = default(L);
            var d = default(D);
            
            LatticeSpan4 x = l.GetLatticeSpan4(_positions.c0, _frequency);
            LatticeSpan4 y = l.GetLatticeSpan4(_positions.c1, _frequency);
            LatticeSpan4 z = l.GetLatticeSpan4(_positions.c2, _frequency);

            float4x2 minima = 2.0f;

            for (int u = -1; u <= 1; u++)
            {
                SmallXXHash4 hx = _hash.Eat(l.ValidateSingleStep(x.p0 + u, _frequency));

                float4 xOffset = u - x.g0;

                for (int v = -1; v <= 1; v++)
                {
                    SmallXXHash4 hy = hx.Eat(l.ValidateSingleStep(y.p0 + v, _frequency));

                    float4 yOffset = v - y.g0;

                    for (int w = -1; w <= 1; w++)
                    {
                        SmallXXHash4 h = hy.Eat(l.ValidateSingleStep(z.p0 + w, _frequency));

                        float4 zOffset = w - z.g0;

                        minima = UpdateVoronoiMinima(minima, d.GetDistance(h.GetBitsAsFloats01(5, 0) + xOffset,
                                                                         h.GetBitsAsFloats01(5, 5) + yOffset,
                                                                         h.GetBitsAsFloats01(5, 10) + zOffset));

                        minima = UpdateVoronoiMinima(minima, d.GetDistance(h.GetBitsAsFloats01(5, 15) + xOffset,
                                                                         h.GetBitsAsFloats01(5, 20) + yOffset,
                                                                         h.GetBitsAsFloats01(5, 25) + zOffset));
                    }
                }
            }

            return default(F).Evaluate(d.Finalize3D(minima));
        }
    }
}