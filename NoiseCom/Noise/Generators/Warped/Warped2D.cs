using System.Numerics;
using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Warped;

[ModelType("Warped 2D")]
public class Warped2D<[ModelHash] THash, TNoiseBase, TNoiseWarp> : INoise<THash, Point2D>
    where THash : IHash<THash>
    where TNoiseBase : INoise<THash, Point2D>
    where TNoiseWarp : INoise<THash, Point2D>
{
    [ModelInline]
    public WarpedSettings Config { get; set; }

    [ModelReference]
    [ModelInjectConstructorArgument("noiseBase")]
    public TNoiseBase BaseNoise { get; }

    [ModelReference]
    [ModelInjectConstructorArgument("noiseWarp")]
    public TNoiseWarp WarpNoise { get; }

    public Warped2D(TNoiseBase noiseBase, TNoiseWarp noiseWarp)
    {
        BaseNoise = noiseBase;
        WarpNoise = noiseWarp;
        Config = WarpedSettings.Default;
    }

    [ModelConstructor]
    public Warped2D(TNoiseBase noiseBase, TNoiseWarp noiseWarp, WarpedSettings config)
    {
        BaseNoise = noiseBase;
        WarpNoise = noiseWarp;
        Config = config;
    }

    public float GetNoise(THash hash, Point2D point, float frequency = 1)
    {
        var warpScale = Config.Power / frequency;
        var warp = Vector2.Zero;
        for (int i = 1; i <= Config.Warps; i++)
        {
            warp = new Vector2()
            {
                X = WarpNoise.GetNoise(hash.Shift(i), point + warp * warpScale, frequency),
                Y = WarpNoise.GetNoise(hash.Shift(-i), point + warp * warpScale, frequency),
            };
        }

        return BaseNoise.GetNoise(hash, point + warp * warpScale, frequency);
    }

    // TODO: implement additional methods to expose warps to the user
    // public float GetNoise(
    //     THash hash,
    //     Point2D point,
    //     float frequency,
    //     out Vector2 firstWarp,
    //     out Vector2 secondWarp
    // )
}
