using System.Numerics;
using System.Runtime.CompilerServices;
using static NoiseCom.Noise.Hash.HashHelper;

namespace NoiseCom.Noise.Hash;

public readonly struct DummyHash8(byte value) : IHash8<DummyHash8>
{
    private readonly byte _value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DummyHash8 Seed(int seed)
    {
        return new();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DummyHash8 Eat(int data)
    {
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte HashByte()
    {
        return _value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextFloat8()
    {
        return ByteToFloat8(_value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DummyHash8 Shift(int offset)
    {
        return this;
    }
}

public readonly struct DummyHash32(uint value) : IHash32<DummyHash32>
{
    private readonly uint _value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DummyHash32 Seed(int seed)
    {
        return new();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DummyHash32 Eat(int data)
    {
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte HashByte()
    {
        return (byte)HashUint();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint HashUint()
    {
        return _value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextFloat24()
    {
        return UintToFloat24(_value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextFloat8()
    {
        return UintToFloat8(_value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector4 NextVector4()
    {
        return UintToFloat8x4(_value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DummyHash32 Shift(int offset)
    {
        return this;
    }
}
