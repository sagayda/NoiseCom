using System.Runtime.CompilerServices;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Voronoi;

[ModelType("F2MinusF1")]
public readonly struct F2MinusF1 : IVoronoiFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Evaluate(float firstMinimum, float secondMinimum, float thirdMinimum)
    {
        return secondMinimum - firstMinimum;
    }
}
