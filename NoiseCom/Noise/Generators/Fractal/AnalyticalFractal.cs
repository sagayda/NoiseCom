using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Fractal;

[ModelType("AnalyticalFractal")]
public class AnalyticalFractal<[ModelHash] THash, [ModelDimension] TPoint, TNoise>
    : Fractal<THash, TPoint, TNoise>,
        IAnalyticalNoise<THash, TPoint>
    where THash : IHash<THash>
    where TPoint : struct, IDimensionalPoint<TPoint>
    where TNoise : IAnalyticalNoise<THash, TPoint>
{
    public AnalyticalFractal(TNoise noise)
        : base(noise) { }

    [ModelConstructor]
    public AnalyticalFractal(TNoise noise, FractalSettings config)
        : base(noise, config) { }

    public TPoint GetDerivative(THash hash, TPoint point, float frequency = 1f)
    {
        float amplitude = 1f,
            amplitudeSum = 0f;
        TPoint sum = default;

        for (int octave = 0; octave < Config.Octaves; octave++)
        {
            sum += Noise.GetDerivative(hash.Shift(octave), point, frequency) * amplitude;
            amplitudeSum += amplitude;
            frequency *= Config.Lacunarity;
            amplitude *= Config.Persistence;
        }

        return sum / amplitudeSum;
    }

    public NoiseSample<TPoint> Sample(THash hash, TPoint point, float frequency = 1f)
    {
        float amplitude = 1f,
            amplitudeSum = 0f;
        NoiseSample<TPoint> sum = default;

        for (int octave = 0; octave < Config.Octaves; octave++)
        {
            sum += Noise.Sample(hash.Shift(octave), point, frequency) * amplitude;
            amplitudeSum += amplitude;
            frequency *= Config.Lacunarity;
            amplitude *= Config.Persistence;
        }

        return sum / amplitudeSum;
    }

    public float GetDerivativeNormalization(float frequency = 1f)
    {
        float derivativeAmplitudeSum = 0f,
            noiseAmplitudeSum = 0f,
            amplitude = 1f;

        for (int octave = 0; octave < Config.Octaves; octave++)
        {
            derivativeAmplitudeSum += amplitude * frequency;
            noiseAmplitudeSum += amplitude;

            frequency *= Config.Lacunarity;
            amplitude *= Config.Persistence;
        }

        return noiseAmplitudeSum / derivativeAmplitudeSum;
    }
}
