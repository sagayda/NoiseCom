using System.Runtime.CompilerServices;
using NoiseCom;

public readonly struct NoiseSample<TPoint>(float value, TPoint derivative)
    where TPoint : struct, IDimensionalPoint<TPoint>
{
    public float Value { get; init; } = value;
    public TPoint Derivatives { get; init; } = derivative;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NoiseSample<TPoint> operator +(
        NoiseSample<TPoint> left,
        NoiseSample<TPoint> right
    ) => new(left.Value + right.Value, left.Derivatives + right.Derivatives);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NoiseSample<TPoint> operator -(
        NoiseSample<TPoint> left,
        NoiseSample<TPoint> right
    ) => new(left.Value - right.Value, left.Derivatives - right.Derivatives);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NoiseSample<TPoint> operator *(NoiseSample<TPoint> left, float right) =>
        new(left.Value * right, left.Derivatives * right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NoiseSample<TPoint> operator /(NoiseSample<TPoint> left, float right) =>
        new(left.Value / right, left.Derivatives / right);
}
