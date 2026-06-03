using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Gradients.TwoDimensional;

[ModelType("OctahedralWeighted")]
public readonly struct OctahedralWeighted<[ModelHash] THash> : IAnalyticalGradient2D<THash>
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
        const float reciprocal7 = 1f / 7f;

        var h = hash.HashByte();
        // Convert the first bytes into the x gradient component
        var ax = (h & 0b111) * reciprocal7;
        // Convert the first bytes into the y gradient component
        var ay = ((h >> 4) & 0b111) * reciprocal7;

        // Scale each component using constant scale factor to achieve octahedral shape
        const float k = 0.292893219f;
        float gx = ((h & (1 << 3)) == 0 ? -ax : ax) * (1f - (k * ay));
        float gy = ((h & (1 << 7)) == 0 ? -ay : ay) * (1f - (k * ax));

        return (gx, gy);
    }
}
