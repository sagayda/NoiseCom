using System.Runtime.CompilerServices;
using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Gradients.OneDimensional;

[ModelType("LinearSolid")]
public readonly struct LinearSolid<[ModelHash] THash> : IAnalyticalGradient1D<THash>
    where THash : IHash8<THash>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Evaluate(THash hash, float x)
    {
        var grad = GetGradient(hash);

        return grad * x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Value, float Dx) EvaluateCombined(THash hash, float x)
    {
        var grad = GetGradient(hash);

        return (grad * x, grad);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float EvaluateDerivatives(THash hash, float x)
    {
        return GetGradient(hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float GetGradient(THash hash)
    {
        const float scale = 1f / 127f;

        var h = hash.HashByte();

        var sign = 1 - ((h & 128) >> 6); // => -1 / 1;
        return sign * (h & 127) * scale; // [+/-] hash / 127 => [+/-][0;1]
    }
}
