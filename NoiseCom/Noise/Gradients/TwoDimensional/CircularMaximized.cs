using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Gradients.TwoDimensional;

[ModelType("CircularMaximized")]
public readonly struct CircularMaximized : IAnalyticalGradient2D<PseudoHash>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Evaluate(PseudoHash hash, float x, float y)
    {
        var (gx, gy) = GetGradient(hash, x, y);

        return (x * gx) + (y * gy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Value, Vector2 Derivatives) EvaluateCombined(PseudoHash hash, float x, float y)
    {
        var (gx, gy) = GetGradient(hash, x, y);

        return ((x * gx) + (y * gy), new(gx, gy));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Value, float Dx, float Dy) EvaluateCombinedScalar(
        PseudoHash hash,
        float x,
        float y
    )
    {
        var (gx, gy) = GetGradient(hash, x, y);

        return ((x * gx) + (y * gy), gx, gy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 EvaluateDerivatives(PseudoHash hash, float x, float y)
    {
        var (gx, gy) = GetGradient(hash, x, y);

        return new(gx, gy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Dx, float Dy) EvaluateDerivativesScalar(PseudoHash hash, float x, float y)
    {
        return GetGradient(hash, x, y);
    }

    public (float Gx, float Gy) GetGradient(PseudoHash hash)
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (float Gx, float Gy) GetGradient(PseudoHash hash, float x, float y)
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

        gx = x >= 0f ? 0.707106781f : -0.707106781f;
        gy = y >= 0f ? 0.707106781f : -0.707106781f;

        // Calculate the vector from the current vertex to the simplex center
        // centerX and centerY must be precalculated before calling Evaluate
        gx = hash.GetData(0) - x;
        gy = hash.GetData(1) - y;

        // Normalize the gradient vector to avoid weight bias in tests
        float length = (float)Math.Sqrt((gx * gx) + (gy * gy));
        if (length > 0f)
        {
            gx /= length;
            gy /= length;
        }

        return (gx, gy);
    }
}
