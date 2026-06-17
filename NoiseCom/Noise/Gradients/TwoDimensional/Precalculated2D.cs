using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Noise.Hash;

namespace NoiseCom.Noise.Gradients.TwoDimensional;

public readonly struct Precalculated2D<THash> : IAnalyticalGradient2D<THash>
    where THash : IHash8<THash>
{
    private readonly float[] _values;

    private Precalculated2D(float[] values)
    {
        _values = values;
    }

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

        return new(gx, gy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Dx, float Dy) EvaluateDerivativesScalar(THash hash, float x, float y)
    {
        return GetGradient(hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float Gx, float Gy) GetGradient(THash hash)
    {
        var h = hash.HashByte();

        return (_values[h], _values[h | 256]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Precalculated2D<THash> Create<TGradient>(TGradient basedOn)
        where TGradient : struct, IGradient2D<DummyHash8>
    {
        float[] buffer = new float[512];

        for (int i = 0; i < 256; i++)
        {
            var (gx, gy) = basedOn.GetGradient(new DummyHash8((byte)i));
            buffer[i] = gx;
            buffer[i | 256] = gy;
        }

        return new(buffer);
    }
}
