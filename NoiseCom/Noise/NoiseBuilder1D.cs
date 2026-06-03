using NoiseCom.Noise.Generators;
using NoiseCom.Noise.Generators.Fractal;
using NoiseCom.Noise.Generators.Perlin;
using NoiseCom.Noise.Generators.Ridged;
using NoiseCom.Noise.Generators.Simplex;
using NoiseCom.Noise.Generators.Voronoi;
using NoiseCom.Noise.Generators.Warped;
using NoiseCom.Noise.Gradients.OneDimensional;
using NoiseCom.Noise.Hash;

namespace NoiseCom.Noise;

public static class NoiseBuilder1D
{
    public static NoiseBuilder1D<THash> For<THash>()
        where THash : IHash<THash>
    {
        return new();
    }

    extension<THash>(NoiseBuilder1D<THash> builder)
        where THash : IHash8<THash>
    {
        public SimplexBuilder1D<THash, LinearSolid<THash>> Simplex()
        {
            return new();
        }

        public PerlinBuilder1D<THash, LinearSolid<THash>> Perlin()
        {
            return new();
        }
    }

    extension<THash>(NoiseBuilder1D<THash> builder)
        where THash : IHash32<THash>
    {
        public VoronoiBuilder1D<THash, F1, Euclidean> Voronoi()
        {
            return new();
        }
    }
}

public class NoiseBuilder1D<THash>
    where THash : IHash<THash>
{
    public FractalBuilder1D<THash, INoise<THash, Point1D>> Fractal()
    {
        return new(null, FractalSettings.Default);
    }

    public RidgedBuilder1D<THash, INoise<THash, Point1D>> Ridged()
    {
        return new(null, RidgedSettings.Default);
    }

    public WarpedBuilder1D<THash, INoise<THash, Point1D>, INoise<THash, Point1D>> Warped()
    {
        return new(null, null, WarpedSettings.Default);
    }
}

public abstract class NoiseBuilder1D<THash, TNoise>
    where THash : IHash<THash>
    where TNoise : INoise<THash, Point1D>
{
    public FractalBuilder1D<THash, TNoise> WrapFractal()
    {
        return new(Build(), FractalSettings.Default);
    }

    public RidgedBuilder1D<THash, TNoise> WrapRidged()
    {
        return new(Build(), RidgedSettings.Default);
    }

    public WarpedBuilder1D<THash, TNoise, INoise<THash, Point1D>> WrapWarped()
    {
        return new(Build(), null, WarpedSettings.Default);
    }

    public abstract TNoise Build();
}

public class PerlinBuilder1D<THash, TGradient> : NoiseBuilder1D<THash, Perlin1D<THash, TGradient>>
    where THash : IHash8<THash>
    where TGradient : struct, IAnalyticalGradient1D<THash>
{
    public override Perlin1D<THash, TGradient> Build()
    {
        return new();
    }

    public PerlinBuilder1D<THash, TNewGradient> WithGradient<TNewGradient>()
        where TNewGradient : struct, IAnalyticalGradient1D<THash>
    {
        return new();
    }
}

public class SimplexBuilder1D<THash, TGradient> : NoiseBuilder1D<THash, Simplex1D<THash, TGradient>>
    where THash : IHash8<THash>
    where TGradient : struct, IAnalyticalGradient1D<THash>
{
    public override Simplex1D<THash, TGradient> Build()
    {
        return new();
    }

    public SimplexBuilder1D<THash, TNewGradient> WithGradient<TNewGradient>()
        where TNewGradient : struct, IAnalyticalGradient1D<THash>
    {
        return new();
    }
}

public class VoronoiBuilder1D<THash, TFunction, TMetric>
    : NoiseBuilder1D<THash, Voronoi1D<THash, TFunction, TMetric>>
    where THash : IHash32<THash>
    where TFunction : struct, IVoronoiFunction
    where TMetric : struct, IVoronoiMetric1D
{
    public override Voronoi1D<THash, TFunction, TMetric> Build()
    {
        return new();
    }

    public VoronoiBuilder1D<THash, TNewFunction, TMetric> WithFunction<TNewFunction>()
        where TNewFunction : struct, IVoronoiFunction
    {
        return new();
    }

    public VoronoiBuilder1D<THash, TFunction, TNewMetric> WithMetric<TNewMetric>()
        where TNewMetric : struct, IVoronoiMetric1D
    {
        return new();
    }
}

public class FractalBuilder1D<THash, TNoiseChild>(TNoiseChild? noiseChild, FractalSettings config)
    : NoiseBuilder1D<THash, Fractal<THash, Point1D, TNoiseChild>>
    where THash : IHash<THash>
    where TNoiseChild : INoise<THash, Point1D>
{
    public FractalSettings Config { get; set; } = config;
    public TNoiseChild? NoiseChild { get; set; } = noiseChild;

    public override Fractal<THash, Point1D, TNoiseChild> Build()
    {
        if (NoiseChild is null)
            throw new InvalidOperationException("'NoiseChild' is requiured");

        return new(NoiseChild, Config);
    }

    public FractalBuilder1D<THash, TNewNoiseChild> WithChildNoise<TNewNoiseChild>(
        TNewNoiseChild noiseChild
    )
        where TNewNoiseChild : INoise<THash, Point1D>
    {
        return new(noiseChild, Config);
    }

    public FractalBuilder1D<THash, TNoiseChild> SetConfig(FractalSettings config)
    {
        Config = config;
        return this;
    }
}

public class RidgedBuilder1D<THash, TNoiseChild>(TNoiseChild? noiseChild, RidgedSettings config)
    : NoiseBuilder1D<THash, Ridged<THash, Point1D, TNoiseChild>>
    where THash : IHash<THash>
    where TNoiseChild : INoise<THash, Point1D>
{
    public RidgedSettings Config { get; set; } = config;
    public TNoiseChild? NoiseChild { get; set; } = noiseChild;

    public override Ridged<THash, Point1D, TNoiseChild> Build()
    {
        if (NoiseChild is null)
            throw new InvalidOperationException("'NoiseChild' is requiured");

        return new(NoiseChild, Config);
    }

    public RidgedBuilder1D<THash, TNewNoiseBase> WithChildNoise<TNewNoiseBase>(
        TNewNoiseBase noiseChild
    )
        where TNewNoiseBase : INoise<THash, Point1D>
    {
        return new(noiseChild, Config);
    }

    public RidgedBuilder1D<THash, TNoiseChild> SetConfig(RidgedSettings config)
    {
        Config = config;
        return this;
    }
}

public class WarpedBuilder1D<THash, TNoiseBase, TNoiseWarp>(
    TNoiseBase? noiseBase,
    TNoiseWarp? noiseWarp,
    WarpedSettings config
) : NoiseBuilder1D<THash, Warped1D<THash, TNoiseBase, TNoiseWarp>>
    where THash : IHash<THash>
    where TNoiseBase : INoise<THash, Point1D>
    where TNoiseWarp : INoise<THash, Point1D>
{
    public WarpedSettings Config { get; set; } = config;
    public TNoiseBase? NoiseBase { get; set; } = noiseBase;
    public TNoiseWarp? NoiseWarp { get; set; } = noiseWarp;

    public override Warped1D<THash, TNoiseBase, TNoiseWarp> Build()
    {
        if (NoiseBase is null)
            throw new InvalidOperationException("'NoiseBase' is requiured");

        if (NoiseWarp is null)
            throw new InvalidOperationException("'NoiseWarp' is requiured");

        return new(NoiseBase, NoiseWarp, Config);
    }

    public WarpedBuilder1D<THash, TNoiseBase, TNoiseWarp> SetConfig(WarpedSettings config)
    {
        Config = config;
        return this;
    }

    public WarpedBuilder1D<THash, TNewNoiseBase, TNoiseWarp> WithBaseNoise<TNewNoiseBase>(
        TNewNoiseBase noiseBase
    )
        where TNewNoiseBase : INoise<THash, Point1D>
    {
        return new(noiseBase, NoiseWarp, Config);
    }

    public WarpedBuilder1D<THash, TNoiseBase, TNewNoiseWarp> WithWarpNoise<TNewNoiseWarp>(
        TNewNoiseWarp noiseWarp
    )
        where TNewNoiseWarp : INoise<THash, Point1D>
    {
        return new(NoiseBase, noiseWarp, Config);
    }
}
