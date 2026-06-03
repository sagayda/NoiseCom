using System.Runtime.CompilerServices;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Voronoi;

[ModelType("Euclidean")]
public readonly struct Euclidean : IVoronoiMetric1D, IVoronoiMetric2D
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Distance(float deltaX)
    {
        return MathF.Abs(deltaX);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Distance(float deltaX, float deltaY)
    {
        return (deltaX * deltaX) + (deltaY * deltaY);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Finalize1D(float distance)
    {
        return distance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Finalize2D(float distance)
    {
        return distance >= 1 ? 1 : MathF.Sqrt(distance);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    float IVoronoiMetric1D.Finalize(float distance) => Finalize1D(distance);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    float IVoronoiMetric2D.Finalize(float distance) => Finalize2D(distance);
}
