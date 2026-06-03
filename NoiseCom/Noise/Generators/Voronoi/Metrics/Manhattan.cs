using System.Runtime.CompilerServices;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Voronoi;

[ModelType("Manhattan")]
public readonly struct Manhattan : IVoronoiMetric1D, IVoronoiMetric2D
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Distance(float deltaX, float deltaY)
    {
        return MathF.Abs(deltaX) + MathF.Abs(deltaY);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Distance(float deltaX)
    {
        return MathF.Abs(deltaX);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Finalize1D(float distance)
    {
        return distance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Finalize2D(float distance)
    {
        return MathF.Min(1f, distance);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    float IVoronoiMetric1D.Finalize(float distance) => Finalize1D(distance);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    float IVoronoiMetric2D.Finalize(float distance) => Finalize2D(distance);
}
