using NoiseCom.Noise.Generators;
using NoiseCom.Noise.Generators.Fractal;
using NoiseCom.Noise.Generators.Perlin;
using NoiseCom.Noise.Generators.Ridged;
using NoiseCom.Noise.Generators.Simplex;
using NoiseCom.Noise.Generators.Voronoi;
using NoiseCom.Noise.Generators.Warped;
using NoiseCom.Noise.Gradients.TwoDimensional;
using NoiseCom.Noise.Hash;

namespace NoiseCom.Noise;

public static class NoiseBuilder2D
{
    public static NoiseBuilder2D<THash> For<THash>()
        where THash : IHash<THash>
    {
        return new();
    }

    extension<THash>(NoiseBuilder2D<THash> builder)
        where THash : IHash8<THash>
    {
        public SimplexBuilder2D<THash, SquareUnitNormalized<THash>> Simplex()
        {
            return new();
        }

        public PerlinBuilder2D<THash, SquareUnitNormalized<THash>> Perlin()
        {
            return new();
        }
    }

    extension<THash>(NoiseBuilder2D<THash> builder)
        where THash : IHash32<THash>
    {
        public VoronoiBuilder2D<THash, F1, Euclidean> Voronoi()
        {
            return new();
        }
    }
}

public class NoiseBuilder2D<THash>
    where THash : IHash<THash>
{
    public FractalBuilder2D<THash, INoise<THash, Point2D>> Fractal()
    {
        return new(null, FractalSettings.Default);
    }

    public RidgedBuilder2D<THash, INoise<THash, Point2D>> Ridged()
    {
        return new(null, RidgedSettings.Default);
    }

    public WarpedBuilder2D<THash, INoise<THash, Point2D>, INoise<THash, Point2D>> Warped()
    {
        return new(null, null, WarpedSettings.Default);
    }
}

public abstract class NoiseBuilder2D<THash, TNoise>
    where THash : IHash<THash>
    where TNoise : INoise<THash, Point2D>
{
    public FractalBuilder2D<THash, TNoise> WrapFractal()
    {
        return new(Build(), FractalSettings.Default);
    }

    public RidgedBuilder2D<THash, TNoise> WrapRidged()
    {
        return new(Build(), RidgedSettings.Default);
    }

    public WarpedBuilder2D<THash, TNoise, INoise<THash, Point2D>> WrapWarped()
    {
        return new(Build(), null, WarpedSettings.Default);
    }

    public abstract TNoise Build();
}

public class PerlinBuilder2D<THash, TGradient> : NoiseBuilder2D<THash, Perlin2D<THash, TGradient>>
    where THash : IHash8<THash>
    where TGradient : struct, IAnalyticalGradient2D<THash>
{
    public override Perlin2D<THash, TGradient> Build()
    {
        return new();
    }

    public PerlinBuilder2D<THash, TNewGradient> WithGradient<TNewGradient>()
        where TNewGradient : struct, IAnalyticalGradient2D<THash>
    {
        return new();
    }
}

public class SimplexBuilder2D<THash, TGradient> : NoiseBuilder2D<THash, Simplex2D<THash, TGradient>>
    where THash : IHash8<THash>
    where TGradient : struct, IAnalyticalGradient2D<THash>
{
    public override Simplex2D<THash, TGradient> Build()
    {
        return new();
    }

    public SimplexBuilder2D<THash, TNewGradient> WithGradient<TNewGradient>()
        where TNewGradient : struct, IAnalyticalGradient2D<THash>
    {
        return new();
    }
}

public class VoronoiBuilder2D<THash, TFunction, TMetric>
    : NoiseBuilder2D<THash, Voronoi2D<THash, TFunction, TMetric>>
    where THash : IHash32<THash>
    where TFunction : struct, IVoronoiFunction
    where TMetric : struct, IVoronoiMetric2D
{
    public override Voronoi2D<THash, TFunction, TMetric> Build()
    {
        return new();
    }

    public VoronoiBuilder2D<THash, TNewFunction, TMetric> WithFunction<TNewFunction>()
        where TNewFunction : struct, IVoronoiFunction
    {
        return new();
    }

    public VoronoiBuilder2D<THash, TFunction, TNewMetric> WithMetric<TNewMetric>()
        where TNewMetric : struct, IVoronoiMetric2D
    {
        return new();
    }
}

public class FractalBuilder2D<THash, TNoiseChild>(TNoiseChild? noiseChild, FractalSettings config)
    : NoiseBuilder2D<THash, Fractal<THash, Point2D, TNoiseChild>>
    where THash : IHash<THash>
    where TNoiseChild : INoise<THash, Point2D>
{
    public FractalSettings Config { get; set; } = config;
    public TNoiseChild? NoiseChild { get; set; } = noiseChild;

    public override Fractal<THash, Point2D, TNoiseChild> Build()
    {
        if (NoiseChild is null)
            throw new InvalidOperationException("'NoiseChild' is requiured");

        return new(NoiseChild, Config);
    }

    public FractalBuilder2D<THash, TNewNoiseChild> WithChildNoise<TNewNoiseChild>(
        TNewNoiseChild noiseChild
    )
        where TNewNoiseChild : INoise<THash, Point2D>
    {
        return new(noiseChild, Config);
    }

    public FractalBuilder2D<THash, TNoiseChild> SetConfig(FractalSettings config)
    {
        Config = config;
        return this;
    }
}

public class RidgedBuilder2D<THash, TNoiseChild>(TNoiseChild? noiseChild, RidgedSettings config)
    : NoiseBuilder2D<THash, Ridged<THash, Point2D, TNoiseChild>>
    where THash : IHash<THash>
    where TNoiseChild : INoise<THash, Point2D>
{
    public RidgedSettings Config { get; set; } = config;
    public TNoiseChild? NoiseChild { get; set; } = noiseChild;

    public override Ridged<THash, Point2D, TNoiseChild> Build()
    {
        if (NoiseChild is null)
            throw new InvalidOperationException("'NoiseChild' is requiured");

        return new(NoiseChild, Config);
    }

    public RidgedBuilder2D<THash, TNewNoiseBase> WithChildNoise<TNewNoiseBase>(
        TNewNoiseBase noiseChild
    )
        where TNewNoiseBase : INoise<THash, Point2D>
    {
        return new(noiseChild, Config);
    }

    public RidgedBuilder2D<THash, TNoiseChild> SetConfig(RidgedSettings config)
    {
        Config = config;
        return this;
    }
}

public class WarpedBuilder2D<THash, TNoiseBase, TNoiseWarp>(
    TNoiseBase? noiseBase,
    TNoiseWarp? noiseWarp,
    WarpedSettings config
) : NoiseBuilder2D<THash, Warped2D<THash, TNoiseBase, TNoiseWarp>>
    where THash : IHash<THash>
    where TNoiseBase : INoise<THash, Point2D>
    where TNoiseWarp : INoise<THash, Point2D>
{
    public WarpedSettings Config { get; set; } = config;
    public TNoiseBase? NoiseBase { get; set; } = noiseBase;
    public TNoiseWarp? NoiseWarp { get; set; } = noiseWarp;

    public override Warped2D<THash, TNoiseBase, TNoiseWarp> Build()
    {
        if (NoiseBase is null)
            throw new InvalidOperationException("'NoiseBase' is requiured");

        if (NoiseWarp is null)
            throw new InvalidOperationException("'NoiseWarp' is requiured");

        return new(NoiseBase, NoiseWarp, Config);
    }

    public WarpedBuilder2D<THash, TNoiseBase, TNoiseWarp> SetConfig(WarpedSettings config)
    {
        Config = config;
        return this;
    }

    public WarpedBuilder2D<THash, TNewNoiseBase, TNoiseWarp> WithBaseNoise<TNewNoiseBase>(
        TNewNoiseBase noiseBase
    )
        where TNewNoiseBase : INoise<THash, Point2D>
    {
        return new(noiseBase, NoiseWarp, Config);
    }

    public WarpedBuilder2D<THash, TNoiseBase, TNewNoiseWarp> WithWarpNoise<TNewNoiseWarp>(
        TNewNoiseWarp noiseWarp
    )
        where TNewNoiseWarp : INoise<THash, Point2D>
    {
        return new(NoiseBase, noiseWarp, Config);
    }
}
