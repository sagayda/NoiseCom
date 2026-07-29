using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Noise.Gradients;
using NoiseCom.Noise.Gradients.TwoDimensional;
using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;
using static NoiseCom.Noise.NoiseHelper;

namespace NoiseCom.Noise.Generators.Simplex;

[ModelType("Simplex 2D")]
public class Simplex2D<[ModelHash] THash, TGradient> : IAnalyticalNoise<THash, Point2D>
    where THash : IHash8<THash>
    where TGradient : struct, IAnalyticalGradient2D<THash>
{
    private const float SkewFactor = 0.366025404f;
    private const float UnskewFactor = 0.211324865f;
    private const float FrequencyNormalization = 0.57735027f;

    // Gradient noise constants
    private const float GradientNormalization = 4.13351392f;
    private const float GradientMaxPartialDerivative = 5.59962938f * FrequencyNormalization;
    private const float GradientMaxGradientMagnitude = 5.72349291f * FrequencyNormalization;

    // Value noise constants
    private const float ValueNormalization = 1f;
    private const float ValueMaxPartialDerivative = 4.20626738f * FrequencyNormalization;
    private const float ValueMaxGradientMagnitude = 4.35464843f * FrequencyNormalization;

    private readonly float _normalization;

    public float MaxPartialDerivative
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    public float MaxGradientMagnitude
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    [ModelTypeReference]
    public TGradient Gradient { get; }

    [ModelConstructor]
    public Simplex2D(TGradient gradient = default)
    {
        Gradient = gradient;

        if (typeof(TGradient) == typeof(Value<THash>))
        {
            _normalization = ValueNormalization;
            MaxPartialDerivative = ValueMaxPartialDerivative;
            MaxGradientMagnitude = ValueMaxGradientMagnitude;
        }
        else
        {
            _normalization = GradientNormalization;
            MaxPartialDerivative = GradientMaxPartialDerivative;
            MaxGradientMagnitude = GradientMaxGradientMagnitude;
        }
    }

    public float GetNoise(THash hash, Point2D point, float frequency = 1f)
    {
        frequency *= FrequencyNormalization; // devide the coordinates by sqrt(3) ~= 0.577350269 to negotiante the skewing impact on the results
        var vPoint = point.Value * frequency;

        float px = vPoint.X;
        float py = vPoint.Y;

        float skew = (px + py) * SkewFactor;

        var skewedX = px + skew;
        var skewedY = py + skew;

        int x0 = FastFloor(skewedX),
            x1 = x0 + 1;
        int y0 = FastFloor(skewedY),
            y1 = y0 + 1;

        var h0 = hash.Eat(x0);
        var h1 = hash.Eat(x1);

        return _normalization
            * (
                Kernel(h0.Eat(y0), x0, y0, px, py)
                + Kernel(h1.Eat(y1), x1, y1, px, py)
                + (
                    (skewedX - x0 > skewedY - y0) // whether the point is in upper or lower simplex (triangle)
                        ? Kernel(h1.Eat(y0), x1, y0, px, py) // and ignore kernel without distribution to the point
                        : Kernel(h0.Eat(y1), x0, y1, px, py)
                )
            );
    }

    public NoiseSample<Point2D> Sample(THash hash, Point2D point, float frequency = 1f)
    {
        frequency *= FrequencyNormalization; // devide the coordinates by sqrt(3) ~= 0.577350269 to negotiante the skewing impact on the results
        var vPoint = point.Value * frequency;

        float px = vPoint.X;
        float py = vPoint.Y;

        float skew = (px + py) * SkewFactor;

        var skewedX = px + skew;
        var skewedY = py + skew;

        int x0 = FastFloor(skewedX),
            x1 = x0 + 1;
        int y0 = FastFloor(skewedY),
            y1 = y0 + 1;

        var h0 = hash.Eat(x0);
        var h1 = hash.Eat(x1);

        var k0 = KernelCombined(h0.Eat(y0), x0, y0, vPoint);
        var k1 = KernelCombined(h1.Eat(y1), x1, y1, vPoint);
        var k2 =
            (skewedX - x0 > skewedY - y0) // whether the point is in upper or lower simplex (triangle)
                ? KernelCombined(h1.Eat(y0), x1, y0, vPoint) // and ignore kernel without distribution to the point
                : KernelCombined(h0.Eat(y1), x0, y1, vPoint);

        return new()
        {
            Value = (k0.Value + k1.Value + k2.Value) * _normalization,
            Derivatives =
                (k0.Derivatives + k1.Derivatives + k2.Derivatives) * (frequency * _normalization),
        };
    }

    public Point2D GetDerivative(THash hash, Point2D point, float frequency = 1f)
    {
        frequency *= FrequencyNormalization; // devide the coordinates by sqrt(3) ~= 0.577350269 to negotiante the skewing impact on the results
        var vPoint = point.Value * frequency;

        float px = vPoint.X;
        float py = vPoint.Y;

        float skew = (px + py) * SkewFactor;

        var skewedX = px + skew;
        var skewedY = py + skew;

        int x0 = FastFloor(skewedX),
            x1 = x0 + 1;
        int y0 = FastFloor(skewedY),
            y1 = y0 + 1;

        var h0 = hash.Eat(x0);
        var h1 = hash.Eat(x1);

        var k0 = KernelDerivative(h0.Eat(y0), x0, y0, vPoint);
        var k1 = KernelDerivative(h1.Eat(y1), x1, y1, vPoint);
        var k2 =
            (skewedX - x0 > skewedY - y0)
                ? KernelDerivative(h1.Eat(y0), x1, y0, vPoint)
                : KernelDerivative(h0.Eat(y1), x0, y1, vPoint);

        return (k0 + k1 + k2) * (frequency * _normalization);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float Kernel(THash hash, float latticeX, float latticeY, float pointX, float pointY)
    {
        float unskew = (latticeX + latticeY) * UnskewFactor;
        var relativeX = pointX - latticeX + unskew;
        var relativeY = pointY - latticeY + unskew;

        float influence = .5f - ((relativeX * relativeX) + (relativeY * relativeY));
        if (influence <= 0f)
            return 0f;

        influence = influence * influence * influence * 8f;

        return influence * Gradient.Evaluate(hash, relativeX, relativeY);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (float Value, Vector2 Derivatives) KernelCombined(
        THash hash,
        float latticeX,
        float latticeY,
        Vector2 point
    )
    {
        float unskew = (latticeX + latticeY) * UnskewFactor;

        var relativeX = point.X - latticeX + unskew;
        var relativeY = point.Y - latticeY + unskew;

        float influence = .5f - ((relativeX * relativeX) + (relativeY * relativeY));

        if (influence <= 0f)
            return (0, Vector2.Zero);

        float influenceSqrScaled = influence * influence * 8f;
        var (value, derivatives) = Gradient.EvaluateCombined(hash, relativeX, relativeY);

        return (
            influenceSqrScaled * value * influence,
            influenceSqrScaled
                * ((derivatives * influence) - (value * 6f * new Vector2(relativeX, relativeY)))
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector2 KernelDerivative(THash hash, float latticeX, float latticeY, Vector2 point)
    {
        float unskew = (latticeX + latticeY) * UnskewFactor;

        var relativeX = point.X - latticeX + unskew;
        var relativeY = point.Y - latticeY + unskew;

        float influence = .5f - ((relativeX * relativeX) + (relativeY * relativeY));

        if (influence <= 0f)
            return Vector2.Zero;

        float influenceSqrScaled = influence * influence * 8f;
        var (value, derivatives) = Gradient.EvaluateCombined(hash, relativeX, relativeY);

        return influenceSqrScaled
            * ((derivatives * influence) - (value * 6f * new Vector2(relativeX, relativeY)));
    }
}
