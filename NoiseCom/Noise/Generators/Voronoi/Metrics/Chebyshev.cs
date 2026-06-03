using System.Runtime.CompilerServices;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Voronoi;

[ModelType("Chebyshev")]
public readonly struct Chebyshev : IVoronoiMetric1D, IVoronoiMetric2D
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Distance(float deltaX)
    {
        return MathF.Abs(deltaX);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Distance(float deltaX, float deltaY)
    {
        var absDeltaX = MathF.Abs(deltaX);
        var absDeltaY = MathF.Abs(deltaY);

        return MathF.Max(absDeltaX, absDeltaY);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Finalize(float distance)
    {
        return distance;
    }
}
