using System.Runtime.CompilerServices;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Voronoi;

[ModelType("F3MinusF1")]
public readonly struct F3MinusF1 : IVoronoiFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Evaluate(float firstMinimum, float secondMinimum, float thirdMinimum)
    {
        return thirdMinimum - firstMinimum;
    }
}
