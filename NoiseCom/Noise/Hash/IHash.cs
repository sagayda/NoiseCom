using System.Numerics;

namespace NoiseCom.Noise.Hash;

// Curiously recurring template pattern is used here
// This technique is required to avoid boxing of struct interface implementations when returning the IHash interface
public interface IHash<out TSelf>
    where TSelf : IHash<TSelf>
{
    public TSelf Eat(int data);
    public TSelf Shift(int offset);

    public static abstract TSelf Seed(int seed);
}

public interface IHash8<TSelf> : IHash<TSelf>
    where TSelf : IHash8<TSelf>
{
    public byte HashByte();

    // TODO: add NextSignedFloat8 for values in the range [-1; 1]
    public float NextFloat8();
}

public interface IHash32<TSelf> : IHash8<TSelf>
    where TSelf : IHash32<TSelf>
{
    public uint HashUint();

    public float NextFloat24();
    public Vector4 NextVector4();
}
