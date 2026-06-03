using System.Runtime.CompilerServices;
using NoiseCom.Noise.Gradients;
using NoiseCom.Noise.Gradients.OneDimensional;
using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;
using static NoiseCom.Noise.NoiseHelper;

namespace NoiseCom.Noise.Generators.Simplex;

[ModelType("Simplex 1D")]
public class Simplex1D<[ModelHash] THash, TGradient> : IAnalyticalNoise<THash, Point1D>
    where THash : IHash8<THash>
    where TGradient : struct, IAnalyticalGradient1D<THash>
{
    private const float ValueBaseNormalization = 1f;
    private const float ValueDerivativeNormalization = ValueBaseNormalization / 3.375f;

    private const float GradientBaseNormalization = 2.37037037f;
    private const float GradientDerivativeNormalization = GradientBaseNormalization / 2.9016204f;

    private readonly float _normalization;
    private readonly float _derivativeNormalization;

    [ModelTypeReference]
    public TGradient Gradient { get; }

    [ModelConstructor]
    public Simplex1D(TGradient gradient = default)
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
        var x = point.Value * frequency;

        int lattice0 = FastFloor(x);
        int lattice1 = lattice0 + 1;

        return Kernel(hash.Eat(lattice0), lattice0, x) + Kernel(hash.Eat(lattice1), lattice1, x);
    }

    public Point1D GetDerivative(THash hash, Point1D point, float frequency = 1f)
    {
        var x = point.Value * frequency;

        int lattice0 = FastFloor(x);
        int lattice1 = lattice0 + 1;

        var k0 = KernelDerivative(hash.Eat(lattice0), lattice0, x);
        var k1 = KernelDerivative(hash.Eat(lattice1), lattice1, x);

        return (k0 + k1) * frequency * _derivativeNormalization;
    }

    public NoiseSample<Point1D> Sample(THash hash, Point1D point, float frequency = 1f)
    {
        var x = point.Value * frequency;

        int lattice0 = FastFloor(x);
        int lattice1 = lattice0 + 1;

        var k0 = KernelCombined(hash.Eat(lattice0), lattice0, x);
        var k1 = KernelCombined(hash.Eat(lattice1), lattice1, x);

        // frequency is here because of the chain rule
        // f(p) = f(frequency(p))
        // f'(p) = f'(frequency(p)) = f'(frequency(p)) * frequency'(p)
        return new()
        {
            Value = (k0.Value + k1.Value) * _normalization,
            Derivatives = (k0.Dx + k1.Dx) * frequency * _derivativeNormalization,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetDerivativeNormalization(float forFrequency = 1f)
    {
        return 1f / forFrequency;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float Kernel(THash hash, float latticePoint, float point)
    {
        float relativePoint = point - latticePoint;
        float influence = 1f - (relativePoint * relativePoint);
        influence = influence * influence * influence;

        return Gradient.Evaluate(hash, relativePoint) * influence * _normalization;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float KernelDerivative(THash hash, float latticePoint, float point)
    {
        float relativePoint = point - latticePoint;
        float influence = 1f - (relativePoint * relativePoint);
        float influenceSqr = influence * influence;

        var (value, dx) = Gradient.EvaluateCombined(hash, relativePoint);

        return influenceSqr * ((dx * influence) - (6f * value * relativePoint));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (float Value, float Dx) KernelCombined(THash hash, float latticePoint, float point)
    {
        // influence(p) = (1 - p^2)^3
        // f(p) = g(p) * influence(p) = g(p) * (1 - p^2)^3
        // dx = f'(p) = g'(p) * influence(p) + g(p) * influence'(p) =
        // = (1 - p^2)^2 * (g'(p) * (1 - p^2) - 6p * g(p))
        float relativePoint = point - latticePoint;
        float influence = 1f - (relativePoint * relativePoint);
        float influenceSqr = influence * influence;

        var (value, dx) = Gradient.EvaluateCombined(hash, relativePoint);

        return (
            value * influenceSqr * influence,
            influenceSqr * ((dx * influence) - (6f * value * relativePoint))
        );
    }
}
