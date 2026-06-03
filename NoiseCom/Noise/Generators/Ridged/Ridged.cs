using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;
using static NoiseCom.Noise.NoiseHelper;

namespace NoiseCom.Noise.Generators.Ridged;

[ModelType("Ridged")]
public class Ridged<[ModelHash] THash, [ModelDimension] TPoint, TNoise> : INoise<THash, TPoint>
    where THash : IHash<THash>
    where TPoint : struct, IDimensionalPoint<TPoint>
    where TNoise : INoise<THash, TPoint>
{
    [ModelInline]
    public RidgedSettings Config { get; set; }

    [ModelReference]
    public TNoise Noise { get; }

    public Ridged(TNoise noise)
    {
        Noise = noise;
        Config = RidgedSettings.Default;
    }

    [ModelConstructor]
    public Ridged(TNoise noise, RidgedSettings config)
    {
        Noise = noise;
        Config = config;
    }

    public float GetNoise(THash hash, TPoint point, float frequency = 1)
    {
        var value = MathF.Abs(Noise.GetNoise(hash, point, frequency));

        switch (Config.Inversion)
        {
            case RidgedInversion.InvertAfterPower:
                value = FastPow(value, Config.Power);
                value = 1f - value;
                break;
            case RidgedInversion.Invert:
                value = 1f - value;
                value = FastPow(value, Config.Power);
                break;
            case RidgedInversion.NoInvert:
            default:
                value = FastPow(value, Config.Power);
                break;
        }

        if (Config.Normalize)
            value = (value * 2f) - 1f;

        return value;
    }
}
