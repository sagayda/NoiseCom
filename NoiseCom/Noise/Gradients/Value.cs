using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Noise.Gradients.OneDimensional;
using NoiseCom.Noise.Gradients.TwoDimensional;
using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Gradients;

[ModelType("Value")]
public readonly struct Value<[ModelHash] THash>
    : IAnalyticalGradient2D<THash>,
        IAnalyticalGradient1D<THash>
    where THash : IHash8<THash>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Evaluate(THash hash, float x, float y)
    {
        return GetValue(hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Evaluate(THash hash, float x)
    {
        return GetValue(hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Value, Vector2 Derivatives) EvaluateCombined(THash hash, float x, float y)
    {
        return (GetValue(hash), Vector2.Zero);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Value, float Dx) EvaluateCombined(THash hash, float x)
    {
        return (GetValue(hash), 0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Value, float Dx, float Dy) EvaluateCombinedScalar(THash hash, float x, float y)
    {
        return (GetValue(hash), 0f, 0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 EvaluateDerivatives(THash hash, float x, float y)
    {
        return Vector2.Zero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float EvaluateDerivatives(THash hash, float x)
    {
        return 0f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Dx, float Dy) EvaluateDerivativesScalar(THash hash, float x, float y)
    {
        return (0f, 0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float GetValue(THash hash)
    {
        return (hash.NextFloat8() * 2f) - 1f; // [0; 1] * 2f - 1f => [-1; 1]
    }
}
