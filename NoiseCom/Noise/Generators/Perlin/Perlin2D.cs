using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Noise.Gradients;
using NoiseCom.Noise.Gradients.TwoDimensional;
using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;
using static NoiseCom.Noise.NoiseHelper;

namespace NoiseCom.Noise.Generators.Perlin;

[ModelType("Perlin 2D")]
public class Perlin2D<[ModelHash] THash, TGradient> : IAnalyticalNoise<THash, Point2D>
    where THash : IHash8<THash>
    where TGradient : struct, IAnalyticalGradient2D<THash>
{
    private const float ValueBaseNormalization = 1f;
    private const float ValueDerivativeNormalization = ValueBaseNormalization / 3.75f;

    private const float GradientBaseNormalization = 1.414213585f;
    private const float GradientDerivativeNormalization = GradientBaseNormalization / 1.9051551f;

    private readonly float _normalization;
    private readonly float _derivativeNormalization;

    [ModelTypeReference]
    public TGradient Gradient { get; }

    [ModelConstructor]
    public Perlin2D(TGradient gradient = default)
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

    public float GetNoise(THash hash, Point2D point, float frequency = 1f)
    {
        var vPoint = point.Value * frequency;

        var xSpan = new LatticeSpan(vPoint.X);
        var ySpan = new LatticeSpan(vPoint.Y);

        THash h0 = hash.Eat(xSpan.Floor),
            h1 = hash.Eat(xSpan.Ceil);

        return Lerp(
                Lerp(
                    Gradient.Evaluate(h0.Eat(ySpan.Floor), xSpan.DeltaFloor, ySpan.DeltaFloor),
                    Gradient.Evaluate(h0.Eat(ySpan.Ceil), xSpan.DeltaFloor, ySpan.DeltaCeil),
                    ySpan.Fade
                ),
                Lerp(
                    Gradient.Evaluate(h1.Eat(ySpan.Floor), xSpan.DeltaCeil, ySpan.DeltaFloor),
                    Gradient.Evaluate(h1.Eat(ySpan.Ceil), xSpan.DeltaCeil, ySpan.DeltaCeil),
                    ySpan.Fade
                ),
                xSpan.Fade
            ) * _normalization;
    }

    public NoiseSample<Point2D> Sample(THash hash, Point2D point, float frequency = 1f)
    {
        var vPoint = point.Value * frequency;

        var xSpan = new LatticeSpan(vPoint.X);
        var ySpan = new LatticeSpan(vPoint.Y);

        THash h0 = hash.Eat(xSpan.Floor),
            h1 = hash.Eat(xSpan.Ceil);

        var g0 = Gradient.EvaluateCombinedScalar(
            h0.Eat(ySpan.Floor),
            xSpan.DeltaFloor,
            ySpan.DeltaFloor
        );
        var g1 = Gradient.EvaluateCombinedScalar(
            h0.Eat(ySpan.Ceil),
            xSpan.DeltaFloor,
            ySpan.DeltaCeil
        );
        var g2 = Gradient.EvaluateCombinedScalar(
            h1.Eat(ySpan.Floor),
            xSpan.DeltaCeil,
            ySpan.DeltaFloor
        );
        var g3 = Gradient.EvaluateCombinedScalar(
            h1.Eat(ySpan.Ceil),
            xSpan.DeltaCeil,
            ySpan.DeltaCeil
        );

        return new()
        {
            Value =
                Lerp(
                    Lerp(g0.Value, g1.Value, ySpan.Fade),
                    Lerp(g2.Value, g3.Value, ySpan.Fade),
                    xSpan.Fade
                ) * _normalization,
            Derivatives =
                new Vector2()
                {
                    X =
                        Lerp(
                            Lerp(g0.Dx, g1.Dx, ySpan.Fade),
                            Lerp(g2.Dx, g3.Dx, ySpan.Fade),
                            xSpan.Fade
                        )
                        + (
                            xSpan.DFade
                            * (
                                Lerp(g2.Value, g3.Value, ySpan.Fade)
                                - Lerp(g0.Value, g1.Value, ySpan.Fade)
                            )
                        ),
                    Y = Lerp(
                        Lerp(g0.Dy, g1.Dy, ySpan.Fade) + (ySpan.DFade * (g1.Value - g0.Value)),
                        Lerp(g2.Dy, g3.Dy, ySpan.Fade) + (ySpan.DFade * (g3.Value - g2.Value)),
                        xSpan.Fade
                    ),
                } * (_derivativeNormalization * frequency),
        };
    }

    public Point2D GetDerivative(THash hash, Point2D point, float frequency = 1f)
    {
        var vPoint = point.Value * frequency;

        var xSpan = new LatticeSpan(vPoint.X);
        var ySpan = new LatticeSpan(vPoint.Y);

        THash h0 = hash.Eat(xSpan.Floor),
            h1 = hash.Eat(xSpan.Ceil);

        var g0 = Gradient.EvaluateCombinedScalar(
            h0.Eat(ySpan.Floor),
            xSpan.DeltaFloor,
            ySpan.DeltaFloor
        );
        var g1 = Gradient.EvaluateCombinedScalar(
            h0.Eat(ySpan.Ceil),
            xSpan.DeltaFloor,
            ySpan.DeltaCeil
        );
        var g2 = Gradient.EvaluateCombinedScalar(
            h1.Eat(ySpan.Floor),
            xSpan.DeltaCeil,
            ySpan.DeltaFloor
        );
        var g3 = Gradient.EvaluateCombinedScalar(
            h1.Eat(ySpan.Ceil),
            xSpan.DeltaCeil,
            ySpan.DeltaCeil
        );

        return new Vector2()
            {
                X =
                    Lerp(Lerp(g0.Dx, g1.Dx, ySpan.Fade), Lerp(g2.Dx, g3.Dx, ySpan.Fade), xSpan.Fade)
                    + (
                        xSpan.DFade
                        * (
                            Lerp(g2.Value, g3.Value, ySpan.Fade)
                            - Lerp(g0.Value, g1.Value, ySpan.Fade)
                        )
                    ),
                Y = Lerp(
                    Lerp(g0.Dy, g1.Dy, ySpan.Fade) + (ySpan.DFade * (g1.Value - g0.Value)),
                    Lerp(g2.Dy, g3.Dy, ySpan.Fade) + (ySpan.DFade * (g3.Value - g2.Value)),
                    xSpan.Fade
                ),
            } * (_derivativeNormalization * frequency);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetDerivativeNormalization(float forFrequency = 1f)
    {
        return 1f / forFrequency;
    }
}
