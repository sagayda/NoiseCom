using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Warped;

[ModelType("Warped 1D")]
public class Warped1D<[ModelHash] THash, TNoiseBase, TNoiseWarp> : INoise<THash, Point1D>
    where THash : IHash<THash>
    where TNoiseBase : INoise<THash, Point1D>
    where TNoiseWarp : INoise<THash, Point1D>
{
    [ModelInline]
    public WarpedSettings Config { get; set; }

    [ModelInjectConstructorArgument("noiseBase")]
    [ModelReference]
    public TNoiseBase BaseNoise { get; }

    [ModelInjectConstructorArgument("noiseWarp")]
    [ModelReference]
    public TNoiseWarp WarpNoise { get; }

    public Warped1D(TNoiseBase noiseBase, TNoiseWarp noiseWarp)
    {
        BaseNoise = noiseBase;
        WarpNoise = noiseWarp;
        Config = WarpedSettings.Default;
    }

    [ModelConstructor]
    public Warped1D(TNoiseBase noiseBase, TNoiseWarp noiseWarp, WarpedSettings config)
    {
        BaseNoise = noiseBase;
        WarpNoise = noiseWarp;
        Config = config;
    }

    public float GetNoise(THash hash, Point1D point, float frequency = 1)
    {
        var warpScale = Config.Power / frequency;
        var warp = 0f;
        for (int i = 1; i <= Config.Warps; i++)
        {
            warp = WarpNoise.GetNoise(hash.Shift(i), point + (warp * warpScale), frequency);
        }

        return BaseNoise.GetNoise(hash, point + (warp * warpScale), frequency);
    }
}
