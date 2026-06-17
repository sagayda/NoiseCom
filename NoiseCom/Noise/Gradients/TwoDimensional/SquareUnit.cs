using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;
using static NoiseCom.Noise.NoiseHelper;

namespace NoiseCom.Noise.Gradients.TwoDimensional;

[ModelType("SquareUnit")]
public readonly struct SquareUnit<[ModelHash] THash> : IAnalyticalGradient2D<THash>
    where THash : IHash8<THash>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Evaluate(THash hash, float x, float y)
    {
        var (gx, gy) = GetGradient(hash);

        return (gx * x) + (gy * y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Value, Vector2 Derivatives) EvaluateCombined(THash hash, float x, float y)
    {
        var (gx, gy) = GetGradient(hash);

        return ((gx * x) + (gy * y), new Vector2(gx, gy));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Value, float Dx, float Dy) EvaluateCombinedScalar(THash hash, float x, float y)
    {
        var (gx, gy) = GetGradient(hash);

        return ((gx * x) + (gy * y), gx, gy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 EvaluateDerivatives(THash hash, float x, float y)
    {
        var (gx, gy) = GetGradient(hash);

        return new Vector2(gx, gy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Dx, float Dy) EvaluateDerivativesScalar(THash hash, float x, float y)
    {
        return GetGradient(hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Gx, float Gy) GetGradient(THash hash)
    {
        var h = hash.NextFloat8();
        float gx = (h * 2f) - 1f;
        float gy = 0.5f - Math.Abs(gx);
        gx -= FastFloor(gx + 0.5f);

        return (gx * 2f, gy * 2f);
        // return new Vector2(gx, gy) * 2f;
    }
}
