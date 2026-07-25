namespace NoiseCom;

// TODO: mirrored / & * operators
public interface IDimensionalPoint<TSelf>
    where TSelf : struct, IDimensionalPoint<TSelf>
{
    public abstract float X { get; }

    public static abstract TSelf operator +(TSelf left, TSelf right);

    public static abstract TSelf operator -(TSelf left, TSelf right);

    public static abstract TSelf operator *(TSelf left, float right);

    public static abstract TSelf operator *(float left, TSelf right);

    public static abstract TSelf operator /(TSelf left, float right);

    public abstract float LengthSquared();
}
