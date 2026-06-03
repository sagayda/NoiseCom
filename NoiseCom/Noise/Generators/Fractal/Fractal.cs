using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Fractal;

[ModelType("Fractal")]
public class Fractal<[ModelHash] THash, [ModelDimension] TPoint, TNoise> : INoise<THash, TPoint>
    where THash : IHash<THash>
    where TPoint : struct, IDimensionalPoint<TPoint>
    where TNoise : INoise<THash, TPoint>
{
    [ModelInline]
    public FractalSettings Config { get; set; }

    [ModelReference]
    public TNoise Noise { get; }

    public Fractal(TNoise noise)
    {
        Noise = noise;
        Config = FractalSettings.Default;
    }

    [ModelConstructor]
    public Fractal(TNoise noise, FractalSettings config)
    {
        Noise = noise;
        Config = config;
    }

    public float GetNoise(THash hash, TPoint point, float frequency = 1f)
    {
        float amplitude = 1f,
            amplitudeSum = 0f,
            // frequency = Config.Frequency,
            sum = 0f;

        for (int octave = 0; octave < Config.Octaves; octave++)
        {
            sum += amplitude * Noise.GetNoise(hash.Shift(octave), point, frequency);
            amplitudeSum += amplitude;
            frequency *= Config.Lacunarity;
            amplitude *= Config.Persistence;
        }

        return sum / amplitudeSum;
    }
}
