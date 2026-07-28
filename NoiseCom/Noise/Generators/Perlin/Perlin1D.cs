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
    private const float GradientNormalization = 2f;
    private const float GradientMaxPartialDerivative = 2.69430126f;

    private const float ValueNormalization = 1f;
    private const float ValueMaxPartialDerivative = 3.75f;

    private readonly float _normalization;

    [ModelTypeReference]
    public TGradient Gradient { get; }

    public float MaxPartialDerivative
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    public float MaxGradientMagnitude => MaxPartialDerivative;

    [ModelConstructor]
    public Perlin1D(TGradient gradient = default)
    {
        Gradient = gradient;

        if (typeof(TGradient) == typeof(Value<THash>))
        {
            _normalization = ValueNormalization;
            MaxPartialDerivative = ValueMaxPartialDerivative;
        }
        else
        {
            _normalization = GradientNormalization;
            MaxPartialDerivative = GradientMaxPartialDerivative;
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
                * _normalization,
        };
    }

    public Point1D GetDerivative(THash hash, Point1D point, float frequency = 1f)
    {
        var span = new LatticeSpan(point.Value * frequency);
        var g0 = Gradient.EvaluateCombined(hash.Eat(span.Floor), span.DeltaFloor);
        var g1 = Gradient.EvaluateCombined(hash.Eat(span.Ceil), span.DeltaCeil);

        return (Lerp(g0.Dx, g1.Dx, span.Fade) + (span.DFade * (g1.Value - g0.Value)))
            * frequency
            * _normalization;
    }
}
