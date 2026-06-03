using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Serialization;
using static NoiseCom.Noise.Hash.HashHelper;

namespace NoiseCom.Noise.Hash;

[ModelType("Squirrel3")]
public readonly struct Squirrel3Hash : IHash32<Squirrel3Hash>
{
    private const uint Noise1 = 0xB5297A4D;
    private const uint Noise2 = 0x68E31DA4;
    private const uint Noise3 = 0x1B56C4E9;

    private readonly uint _accumulator;

    public Squirrel3Hash(int seed)
    {
        _accumulator = (uint)seed;
    }

    private Squirrel3Hash(uint accumulator)
    {
        _accumulator = accumulator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Squirrel3Hash Seed(int seed)
    {
        return new(seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Squirrel3Hash Eat(int data)
    {
        var mangled = (uint)data;
        mangled *= Noise1;
        mangled += _accumulator;
        mangled ^= mangled >> 8;
        mangled += Noise2;
        mangled ^= mangled << 8;
        mangled *= Noise3;
        mangled ^= mangled >> 8;

        return new(mangled);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte HashByte()
    {
        return (byte)_accumulator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint HashUint()
    {
        return _accumulator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextFloat24()
    {
        return UintToFloat24(_accumulator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextFloat8()
    {
        return UintToFloat8(_accumulator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector4 NextVector4()
    {
        return UintToFloat8x4(_accumulator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Squirrel3Hash Shift(int offset)
    {
        return new((uint)offset + _accumulator);
    }
}
