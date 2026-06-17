using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Gradients.TwoDimensional;

[ModelType("CircularMaximized")]
public readonly struct CircularMaximized<[ModelHash] THash> : IAnalyticalGradient2D<THash>
    where THash : struct, IHash8<THash>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Evaluate(THash hash, float x, float y)
    {
        var (gx, gy) = GetGradient(x, y);

        return (x * gx) + (y * gy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Value, Vector2 Derivatives) EvaluateCombined(THash hash, float x, float y)
    {
        var (gx, gy) = GetGradient(x, y);

        return ((x * gx) + (y * gy), new(gx, gy));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Value, float Dx, float Dy) EvaluateCombinedScalar(THash hash, float x, float y)
    {
        var (gx, gy) = GetGradient(x, y);

        return ((x * gx) + (y * gy), gx, gy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 EvaluateDerivatives(THash hash, float x, float y)
    {
        var (gx, gy) = GetGradient(x, y);

        return new(gx, gy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Dx, float Dy) EvaluateDerivativesScalar(THash hash, float x, float y)
    {
        return GetGradient(x, y);
    }

    public (float Gx, float Gy) GetGradient(THash hash)
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (float Gx, float Gy) GetGradient(float x, float y)
    {
        // var gx = point.X / MathF.Sqrt((point.X * point.X) + (point.Y * point.Y));
        // var gy = point.Y / MathF.Sqrt((point.X * point.X) + (point.Y * point.Y));
        var reciprocalLength = MathF.ReciprocalEstimate((x * x) + (y * y));

        var gx = x * reciprocalLength;
        var gy = y * reciprocalLength;
        // float k = 0.292893219f;
        // gx = (1f - k * point.Y) * point.X;
        // gy = (1f - k * point.X) * point.Y;

        // return gx + gy;
        // return gx * point.X + gy * point.Y;

        return (gx, gy);
    }
}
