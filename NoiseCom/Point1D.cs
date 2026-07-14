using System.Runtime.CompilerServices;
using NoiseCom.Serialization;

namespace NoiseCom;

[ModelType("1D")]
public readonly struct Point1D(float value) : IDimensionalPoint<Point1D>
{
    public readonly float Value = value;

    public readonly float X
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point1D operator +(Point1D left, Point1D right) => new(left.Value + right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point1D operator -(Point1D left, Point1D right) => new(left.Value - right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point1D operator *(Point1D left, float right) => new(left.Value * right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point1D operator *(float left, Point1D right) => new(right.Value * left);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point1D operator /(Point1D left, float right) => new(left.Value / right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point1D(float value) => new(value);
}
