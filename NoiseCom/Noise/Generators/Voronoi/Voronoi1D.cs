using System.Runtime.CompilerServices;
using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;
using static NoiseCom.Noise.NoiseHelper;

namespace NoiseCom.Noise.Generators.Voronoi;

[ModelType("Voronoi 1D")]
public class Voronoi1D<[ModelHash] THash, TFunction, TMetric> : INoise<THash, Point1D>
    where THash : IHash32<THash>
    where TFunction : struct, IVoronoiFunction
    where TMetric : struct, IVoronoiMetric1D
{
    [ModelTypeReference]
    public TFunction Function { get; }

    [ModelTypeReference]
    public TMetric Metric { get; }

    [ModelConstructor]
    public Voronoi1D(TFunction function = default, TMetric metric = default)
    {
        Function = function;
        Metric = metric;
    }

    public float GetNoise(THash hash, Point1D point, float frequency = 1)
    {
        var vPoint = point.Value * frequency;

        var latticePoint = FastFloor(vPoint);

        var relativePoint = vPoint - latticePoint;

        float firstMinimum = float.MaxValue,
            secondMinimum = float.MaxValue,
            thirdMinimum = float.MaxValue;
        for (int i = -1; i <= 1; i++)
        {
            var latticeDeltaPoint = relativePoint - i;
            // Point1 = (features.X)
            // Point2 = (features.Y)
            var features = hash.Eat(latticePoint + i).NextVector4();

            var distance = Metric.Distance(features.X - latticeDeltaPoint);
            InsertionSortOfThree(distance, ref firstMinimum, ref secondMinimum, ref thirdMinimum);

            distance = Metric.Distance(features.Y - latticeDeltaPoint);
            InsertionSortOfThree(distance, ref firstMinimum, ref secondMinimum, ref thirdMinimum);
        }

        return (
                Function.Evaluate(
                    Metric.Finalize(firstMinimum),
                    Metric.Finalize(secondMinimum),
                    Metric.Finalize(thirdMinimum)
                ) * 2f
            ) - 1f;
    }
}
