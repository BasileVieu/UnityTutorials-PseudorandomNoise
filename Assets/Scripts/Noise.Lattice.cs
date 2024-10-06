using Unity.Mathematics;
using static Unity.Mathematics.math;

public static partial class Noise
{
    public struct LatticeSpan4
    {
        public int4 p0;
        public int4 p1;
        public float4 g0;
        public float4 g1;
        public float4 t;
    }

    public interface ILattice
    {
        LatticeSpan4 GetLatticeSpan4(float4 _coordinates, int _frequency);

        int4 ValidateSingleStep(int4 _points, int _frequency);
    }

    public struct LatticeNormal : ILattice
    {
        public LatticeSpan4 GetLatticeSpan4(float4 _coordinates, int _frequency)
        {
            _coordinates *= _frequency;

            float4 points = floor(_coordinates);

            LatticeSpan4 span;
            span.p0 = (int4) points;
            span.p1 = span.p0 + 1;
            span.g0 = _coordinates - span.p0;
            span.g1 = span.g0 - 1.0f;
            span.t = _coordinates - points;
            span.t = span.t * span.t * span.t * (span.t * (span.t * 6.0f - 15.0f) + 10.0f);

            return span;
        }

        public int4 ValidateSingleStep(int4 _points, int _frequency) => _points;
    }

    public struct LatticeTiling : ILattice
    {
        public LatticeSpan4 GetLatticeSpan4(float4 _coordinates, int _frequency)
        {
            _coordinates *= _frequency;

            float4 points = floor(_coordinates);

            LatticeSpan4 span;
            span.p0 = (int4) points;
            span.g0 = _coordinates - span.p0;
            span.g1 = span.g0 - 1.0f;

            span.p0 -= (int4) (points / _frequency) * _frequency;
            span.p0 = select(span.p0, span.p0 + _frequency, span.p0 < 0);
            span.p1 = span.p0 + 1;
            span.p1 = select(span.p1, 0, span.p1 == _frequency);
            
            span.t = _coordinates - points;
            span.t = span.t * span.t * span.t * (span.t * (span.t * 6.0f - 15.0f) + 10.0f);

            return span;
        }

        public int4 ValidateSingleStep(int4 _points, int _frequency) => select(select(_points, 0, _points == _frequency), _frequency - 1, _points == -1);
    }

    public struct Lattice1D<L, G> : INoise where L : struct, ILattice where G : struct, IGradient
    {
        public float4 GetNoise4(float4x3 _positions, SmallXXHash4 _hash, int _frequency)
        {
            LatticeSpan4 x = default(L).GetLatticeSpan4(_positions.c0, _frequency);

            var g = default(G);

            return g.EvaluateCombined(lerp(g.Evaluate(_hash.Eat(x.p0), x.g0),
                                                     g.Evaluate(_hash.Eat(x.p1), x.g1), x.t));
        }
    }

    public struct Lattice2D<L, G> : INoise where L : struct, ILattice where G : struct, IGradient
    {
        public float4 GetNoise4(float4x3 _positions, SmallXXHash4 _hash, int _frequency)
        {
            var l = default(L);
            
            LatticeSpan4 x = l.GetLatticeSpan4(_positions.c0, _frequency);
            LatticeSpan4 z = l.GetLatticeSpan4(_positions.c2, _frequency);

            SmallXXHash4 h0 = _hash.Eat(x.p0);
            SmallXXHash4 h1 = _hash.Eat(x.p1);

            var g = default(G);

            return g.EvaluateCombined(lerp(lerp(g.Evaluate(h0.Eat(z.p0), x.g0, z.g0),
                                                          g.Evaluate(h0.Eat(z.p1), x.g0, z.g1),
                                                          z.t),
                                                     lerp(g.Evaluate(h1.Eat(z.p0), x.g1, z.g0),
                                                          g.Evaluate(h1.Eat(z.p1), x.g1, z.g1),
                                                          z.t),
                                                     x.t));
        }
    }

    public struct Lattice3D<L, G> : INoise where L : struct, ILattice where G : struct, IGradient
    {
        public float4 GetNoise4(float4x3 _positions, SmallXXHash4 _hash, int _frequency)
        {
            var l = default(L);
            
            LatticeSpan4 x = l.GetLatticeSpan4(_positions.c0, _frequency);
            LatticeSpan4 y = l.GetLatticeSpan4(_positions.c1, _frequency);
            LatticeSpan4 z = l.GetLatticeSpan4(_positions.c2, _frequency);

            SmallXXHash4 h0 = _hash.Eat(x.p0);
            SmallXXHash4 h1 = _hash.Eat(x.p1);
            SmallXXHash4 h00 = h0.Eat(y.p0);
            SmallXXHash4 h01 = h0.Eat(y.p1);
            SmallXXHash4 h10 = h1.Eat(y.p0);
            SmallXXHash4 h11 = h1.Eat(y.p1);

            var g = default(G);

            return g.EvaluateCombined(lerp(lerp(lerp(g.Evaluate(h00.Eat(z.p0), x.g0, y.g0, z.g0),
                                                               g.Evaluate(h00.Eat(z.p1), x.g0, y.g0, z.g1),
                                                               z.t),
                                                          lerp(g.Evaluate(h01.Eat(z.p0), x.g0, y.g1, z.g0),
                                                               g.Evaluate(h01.Eat(z.p1), x.g0, y.g1, z.g1),
                                                               z.t),
                                                          y.t),
                                                     lerp(lerp(g.Evaluate(h10.Eat(z.p0), x.g1, y.g0, z.g0),
                                                               g.Evaluate(h10.Eat(z.p1), x.g1, y.g0, z.g1),
                                                               z.t),
                                                          lerp(g.Evaluate(h11.Eat(z.p0), x.g1, y.g1, z.g0),
                                                               g.Evaluate(h11.Eat(z.p1), x.g1, y.g1, z.g1),
                                                               z.t),
                                                          y.t),
                                                     x.t));
        }
    }
}