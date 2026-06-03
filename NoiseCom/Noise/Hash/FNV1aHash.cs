using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Serialization;
using static NoiseCom.Noise.Hash.HashHelper;

namespace NoiseCom.Noise.Hash;

[ModelType("FNV1a")]
public readonly struct FNV1aHash : IHash32<FNV1aHash>
{
    private const uint OffsetBasis = 0x811C9DC5;
    private const uint Prime = 0x1000193;

    private readonly uint _accumulator;

    private FNV1aHash(uint accumulator)
    {
        _accumulator = accumulator;
    }

    public FNV1aHash()
    {
        _accumulator = OffsetBasis;
    }

    public FNV1aHash(int seed)
    {
        uint hash = OffsetBasis;
        uint uData = (uint)seed;

        // first byte
        hash ^= uData & 0xff;
        hash *= Prime;

        // second byte
        hash ^= (uData >> 8) & 0xff;
        hash *= Prime;

        // third byte
        hash ^= (uData >> 16) & 0xff;
        hash *= Prime;

        // fourth byte
        hash ^= (uData >> 24) & 0xff;
        hash *= Prime;

        _accumulator = hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FNV1aHash Seed(int seed)
    {
        return new(seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FNV1aHash Eat(int data)
    {
        uint hash = _accumulator;
        uint uData = (uint)data;

        // first byte
        hash ^= uData & 0xff;
        hash *= Prime;

        // second byte
        hash ^= (uData >> 8) & 0xff;
        hash *= Prime;

        // third byte
        hash ^= (uData >> 16) & 0xff;
        hash *= Prime;

        // fourth byte
        hash ^= uData >> 24;
        hash *= Prime;

        return new(hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FNV1aHash Eat(byte data)
    {
        uint hash = _accumulator;

        hash ^= data;
        hash *= Prime;

        return new(hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FNV1aHash Shift(int offset)
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
