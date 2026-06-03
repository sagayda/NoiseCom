using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Serialization;
using static NoiseCom.Noise.Hash.HashHelper;

namespace NoiseCom.Noise.Hash;

[ModelType("MurMur3")]
public readonly struct MurMur3Hash : IHash32<MurMur3Hash>
{
    private const uint Prime1 = 0XCC9E2D51;
    private const uint Prime2 = 0X1B873593;
    private const uint Prime3 = 0x5;
    private const uint Avalanche1 = 0x85EBCA6B;
    private const uint Avalanche2 = 0xC2B2AE35;
    private const uint N = 0XE6546B64;

    private readonly uint _accumulator;

    private MurMur3Hash(uint accumulator)
    {
        _accumulator = accumulator;
    }

    public MurMur3Hash(int seed)
    {
        _accumulator = (uint)seed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MurMur3Hash Seed(int seed)
    {
        return new(seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MurMur3Hash Eat(int data)
    {
        var hash = _accumulator;
        uint uData = (uint)data;

        uData *= Prime1;
        uData = BitOperations.RotateLeft(uData, 15);
        uData *= Prime2;

        hash ^= uData;
        hash = BitOperations.RotateLeft(hash, 13);
        hash = (hash * Prime3) + N;

        return new(hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MurMur3Hash Shift(int offset)
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
        var avalanche = _accumulator;

        avalanche ^= avalanche >> 16;
        avalanche *= Avalanche1;
        avalanche ^= avalanche >> 13;
        avalanche *= Avalanche2;
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
