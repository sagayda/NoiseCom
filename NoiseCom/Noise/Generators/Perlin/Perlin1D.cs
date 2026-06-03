using System.Runtime.CompilerServices;
using NoiseCom.Noise.Gradients;
using NoiseCom.Noise.Gradients.OneDimensional;
using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;
using static NoiseCom.Noise.NoiseHelper;

namespace NoiseCom.Noise.Generators.Perlin;

[ModelType("Perlin 1D")]
public class Perlin1D<[ModelHash] THash, TGradient> : IAnalyticalNoise<THash, Point1D>
    where THash : IHash8<THash>
    where TGradient : struct, IAnalyticalGradient1D<THash>
{
    private const float ValueBaseNormalization = 1f;
    private const float ValueDerivativeNormalization = ValueBaseNormalization / 3.75f;

    private const float GradientBaseNormalization = 2f;
    private const float GradientDerivativeNormalization =
        GradientBaseNormalization / 1.5366563145999f;

    private readonly float _normalization;
    private readonly float _derivativeNormalization;

    [ModelTypeReference]
    public TGradient Gradient { get; }

    [ModelConstructor]
    public Perlin1D(TGradient gradient = default)
    {
        Gradient = gradient;

        if (gradient is Value<THash>)
        {
            _normalization = ValueBaseNormalization;
            _derivativeNormalization = ValueDerivativeNormalization;
        }
        else
        {
            _normalization = GradientBaseNormalization;
            _derivativeNormalization = GradientDerivativeNormalization;
        }
    }

    public float GetNoise(THash hash, Point1D point, float frequency = 1f)
    {
        var span = new LatticeSpan(point.Value * frequency);

        return Lerp(
                Gradient.Evaluate(hash.Eat(span.Floor), span.DeltaFloor),
                Gradient.Evaluate(hash.Eat(span.Ceil), span.DeltaCeil),
                span.Fade
            ) * _normalization;
    }

    public NoiseSample<Point1D> Sample(THash hash, Point1D point, float frequency = 1f)
    {
        var span = new LatticeSpan(point.Value * frequency);
        var g0 = Gradient.EvaluateCombined(hash.Eat(span.Floor), span.DeltaFloor);
        var g1 = Gradient.EvaluateCombined(hash.Eat(span.Ceil), span.DeltaCeil);

        return new()
        {
            Value = Lerp(g0.Value, g1.Value, span.Fade) * _normalization,
            Derivatives =
                (Lerp(g0.Dx, g1.Dx, span.Fade) + (span.DFade * (g1.Value - g0.Value)))
                * frequency
                * _derivativeNormalization,
        };
    }

    public Point1D GetDerivative(THash hash, Point1D point, float frequency = 1f)
    {
        var span = new LatticeSpan(point.Value * frequency);
        var g0 = Gradient.EvaluateCombined(hash.Eat(span.Floor), span.DeltaFloor);
        var g1 = Gradient.EvaluateCombined(hash.Eat(span.Ceil), span.DeltaCeil);

        return (Lerp(g0.Dx, g1.Dx, span.Fade) + (span.DFade * (g1.Value - g0.Value)))
            * frequency
            * _derivativeNormalization;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetDerivativeNormalization(float forFrequency = 1f)
    {
        return 1f / forFrequency;
    }
}
