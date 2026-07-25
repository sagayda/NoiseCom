using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Fractal;

[ModelType("Fractal")]
public class DerivativeModulatedFractal<[ModelHash] THash, [ModelDimension] TPoint, TNoise>
    : INoise<THash, TPoint>
    where THash : IHash<THash>
    where TPoint : struct, IDimensionalPoint<TPoint>
    where TNoise : IAnalyticalNoise<THash, TPoint>
{
    [ModelInline]
    public FractalSettings Config { get; set; }

    [ModelReference]
    public TNoise Noise { get; }

    public DerivativeModulatedFractal(TNoise noise)
    {
        Noise = noise;
        Config = FractalSettings.Default;
    }

    [ModelConstructor]
    public DerivativeModulatedFractal(TNoise noise, FractalSettings config)
    {
        Noise = noise;
        Config = config;
    }

    public float GetNoise(THash hash, TPoint point, float frequency = 1f)
    {
        float amplitude = 1f,
            amplitudeSum = 0f,
            // frequency = Config.Frequency,
            valueSum = 0f,
            initialFrequency = frequency;

        TPoint derivativeSum = new();

        // var s = Noise.Sample(hash, point, frequency);
        // return (s.Derivatives / frequency).LengthSquared();

        for (int octave = 0; octave < Config.Octaves; octave++)
        {
            var sample = Noise.Sample(hash.Shift(octave), point, frequency);

            valueSum += amplitude * sample.Value;

            var weight = (sample.Derivatives / frequency).LengthSquared();
            weight = (weight + 2f) / 7f;
            // return sample.Derivatives.X * (1f / frequency);
            // return sample.Derivatives.X / frequency * 0.420014064f;
            return sample.Value;
            // return sample.Derivatives.X / frequency;
            // return weight;
            // weight = (weight * weight) * (3f - 2f * weight);
            // weight = weight * weight;
            // weight = (weight + 0f) / (weight + 2f);
            // return weight;

            // float weight = 1f / (1f + (derivativeSum / initialFrequency).LengthSquared());
            // float weight = (derivativeSum / initialFrequency).LengthSquared() * 0.2f;
            // weight = 1f / (1f + weight);

            // float weight = (derivativeSum / initialFrequency).LengthSquared();
            // weight = 1f / (1f + weight);
            // weight = (weight + 0f) / (weight + 1f);
            // weight = weight / (weight + 1f);
            // if (octave == 0)
            //     weight = 1f;
            // else
            //     return weight;

            // weight = weight * weight * weight;

            // valueSum += amplitude * sample.Value * weight;
            // valueSum += amplitude * Noise.GetNoise(hash.Shift(octave), point, frequency);
            //
            // derivativeSum += amplitude * sample.Derivatives;

            amplitudeSum += amplitude;
            frequency *= Config.Lacunarity;
            amplitude *= Config.Persistence * weight;
        }

        return valueSum / amplitudeSum;
    }
}
