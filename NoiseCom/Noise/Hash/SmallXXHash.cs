using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Serialization;
using static NoiseCom.Noise.Hash.HashHelper;

namespace NoiseCom.Noise.Hash;

[ModelType("SmallXX")]
public readonly struct SmallXXHash : IHash32<SmallXXHash>
{
    private const uint Prime1 = 0x9E3779B1;
    private const uint Prime2 = 0x85EBCA77;
    private const uint Prime3 = 0xC2B2AE3D;
    private const uint Prime4 = 0x27D4EB2F;
    private const uint Prime5 = 0x165667B1;

    private readonly uint _accumulator;

    public SmallXXHash(int seed)
    {
        _accumulator = (uint)seed + Prime5;
    }

    private SmallXXHash(uint accumulator)
    {
        _accumulator = accumulator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SmallXXHash Seed(int seed)
    {
        return new(seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SmallXXHash Eat(int data)
    {
        return new(BitOperations.RotateLeft(_accumulator + ((uint)data * Prime3), 17) * Prime4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SmallXXHash Eat(byte data)
    {
        return new(BitOperations.RotateLeft(_accumulator + (data * Prime5), 11) * Prime1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SmallXXHash Shift(int offset)
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
        uint avalanche = _accumulator;
        avalanche ^= avalanche >> 15;
        avalanche *= Prime2;
        avalanche ^= avalanche >> 13;
        avalanche *= Prime3;
        avalanche ^= avalanche >> 16;
        return avalanche;
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
