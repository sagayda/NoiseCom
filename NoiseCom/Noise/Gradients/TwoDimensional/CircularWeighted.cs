using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Gradients.TwoDimensional;

// TODO: configurable bytes count for angle and length
[ModelType("CircularWeighted")]
public readonly struct CircularWeighted<[ModelHash] THash> : IAnalyticalGradient2D<THash>
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

        return ((gx * x) + (gy * y), new(gx, gy));
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

        return new(gx, gy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Dx, float Dy) EvaluateDerivativesScalar(THash hash, float x, float y)
    {
        return GetGradient(hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (float Gx, float Gy) GetGradient(THash hash)
    {
        const float recipocal32 = 1f / 32f;
        const float recipocal7 = 1f / 7f;

        var h = hash.HashByte();

        // convert the first bytes into the angle
        float angle = (h & 0b11111) * recipocal32 * 2f * MathF.PI; // (h & 0b11111) / 32 * 2 * PI
        // convert the remaining bytes into the length
        float length = (h >> 5) * recipocal7; // (h & 0b111) / 7f

        // NOTE: square length is not necessary
        // This only changes the length distribution
        // Can be used to control the amount of flat regions in the resulting noise
        length *= length;

        (float gx, float gy) = MathF.SinCos(angle);

        return (gx * length, gy * length);
    }
}
