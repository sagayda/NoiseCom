using System.Runtime.CompilerServices;
using NoiseCom.Noise.Hash;

namespace NoiseCom.Noise.Gradients.OneDimensional;

public readonly struct Precalculated1D<THash> : IAnalyticalGradient1D<THash>
    where THash : IHash8<THash>
{
    private readonly float[] _values;

    private Precalculated1D(float[] values)
    {
        _values = values;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetGradient(THash hash)
    {
        var h = hash.HashByte();

        return _values[h];
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
    public float Evaluate(THash hash, float x)
    {
        var grad = GetGradient(hash);

        return grad * x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Precalculated1D<THash> Create<TGradient>(TGradient basedOn)
        where TGradient : struct, IGradient1D<DummyHash8>
    {
        float[] buffer = new float[256];

        for (int i = 0; i < 256; i++)
            buffer[i] = basedOn.GetGradient(new DummyHash8((byte)i));

        return new(buffer);
    }
}
