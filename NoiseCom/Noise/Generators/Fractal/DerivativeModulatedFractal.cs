using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Fractal;

[ModelType("DerivativeModulatedFractal")]
public class DerivativeModulatedFractal<[ModelHash] THash, [ModelDimension] TPoint, TNoise>
    : INoise<THash, TPoint>
    where THash : IHash<THash>
    where TPoint : struct, IDimensionalPoint<TPoint>
    where TNoise : IAnalyticalNoise<THash, TPoint>
{
    [ModelInline]
    public DerivativeModulatedFractalSettings Config { get; set; }

    [ModelReference]
    public TNoise Noise { get; }

    public DerivativeModulatedFractal(TNoise noise)
    {
        Noise = noise;
        Config = DerivativeModulatedFractalSettings.Default;
    }

    [ModelConstructor]
    public DerivativeModulatedFractal(TNoise noise, DerivativeModulatedFractalSettings config)
    {
        Noise = noise;
        Config = config;
    }

    public float GetNoise(THash hash, TPoint point, float frequency = 1f)
    {
        float amplitude = 1f,
            amplitudeSum = 0f,
            sum = 0f;

        for (int octave = 0; octave < Config.Octaves; octave++)
        {
            var sample = Noise.Sample(hash.Shift(octave), point, frequency);
            sum += amplitude * sample.Value;

            var derivativeNorm = 1f / (frequency * Noise.MaxGradientMagnitude);
            var weight = (sample.Derivatives * derivativeNorm).LengthSquared();

            // EffectiveMaximum here allows to stretch the most statistically often range of values (eg. [0; 0.8], bigger gradients are very rare) to the range [0; 1]
            // if Invert == false, we cut off the top (eg. [0.8; 1])
            // else we cut off the inverted bottom (eg. [0; 0.2])
            weight = Config.Invert ? (-weight - 1f - Config.EffectiveMaximum) : weight;
            weight /= Config.EffectiveMaximum;
            // also we need to clamp the result to avoid artifacts on low EffectiveMaximum values
            weight = Math.Clamp(weight, 0f, 1f);

            amplitudeSum += amplitude;
            frequency *= Config.Lacunarity;
            amplitude *= Config.Persistence * weight;
        }

        return sum / amplitudeSum;
    }
}
