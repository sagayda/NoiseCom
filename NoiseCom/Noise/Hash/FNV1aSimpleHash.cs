using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Serialization;
using static NoiseCom.Noise.Hash.HashHelper;

namespace NoiseCom.Noise.Hash;

[ModelType("FNV1aSimple")]
public readonly struct FNV1aSimpleHash : IHash32<FNV1aSimpleHash>
{
    private const uint OffsetBasis = 0x811C9DC5;
    private const uint Prime = 0x1000193;

    private readonly uint _accumulator;

    private FNV1aSimpleHash(uint accumulator)
    {
        _accumulator = accumulator;
    }

    public FNV1aSimpleHash()
    {
        _accumulator = OffsetBasis;
    }

    public FNV1aSimpleHash(int seed)
    {
        uint hash = OffsetBasis;

        hash ^= (uint)seed;
        hash *= Prime;

        _accumulator = hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FNV1aSimpleHash Seed(int seed)
    {
        return new(seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FNV1aSimpleHash Eat(int data)
    {
        uint hash = _accumulator;

        hash ^= (uint)data;
        hash *= Prime;

        return new(hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FNV1aSimpleHash Eat(byte data)
    {
        uint hash = _accumulator;

        hash ^= data;
        hash *= Prime;

        return new(hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FNV1aSimpleHash Shift(int offset)
    {
        return new(_accumulator + (uint)offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte HashByte()
    {
        return (byte)HashUint();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint HashUint()
    {
        return _accumulator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextFloat24()
    {
        return UintToFloat24(HashUint());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextFloat8()
    {
        return UintToFloat8(HashUint());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector4 NextVector4()
    {
        return UintToFloat8x4(HashUint());
    }
}
