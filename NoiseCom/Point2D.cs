using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseCom.Serialization;

namespace NoiseCom;

[ModelType("2D")]
public readonly struct Point2D : IDimensionalPoint<Point2D>
{
    public readonly Vector2 Value;

    public readonly float X
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value.X;
    }

    public readonly float Y
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value.Y;
    }

    public Point2D(Vector2 value)
    {
        Value = value;
    }

    public Point2D(float x, float y)
    {
        Value = new Vector2(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2D operator +(Point2D left, Point2D right) => new(left.Value + right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2D operator -(Point2D left, Point2D right) => new(left.Value - right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2D operator *(Point2D left, float right) => new(left.Value * right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2D operator *(float left, Point2D right) => new(right.Value * left);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2D operator /(Point2D left, float right) => new(left.Value / right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point2D(Vector2 value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float LengthSquared() => (X * X) + (Y * Y);
}
