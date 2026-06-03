using NoiseCom.Noise.Hash;
using NoiseCom.Serialization;
using static NoiseCom.Noise.NoiseHelper;

namespace NoiseCom.Noise.Generators.Voronoi;

[ModelType("Voronoi 2D")]
public class Voronoi2D<[ModelHash] THash, TFunction, TMetric> : INoise<THash, Point2D>
    where THash : IHash32<THash>
    where TFunction : struct, IVoronoiFunction
    where TMetric : struct, IVoronoiMetric2D
{
    [ModelTypeReference]
    public TFunction Function { get; }

    [ModelTypeReference]
    public TMetric Metric { get; }

    [ModelConstructor]
    public Voronoi2D(TFunction function = default, TMetric metric = default)
    {
        Function = function;
        Metric = metric;
    }

    public float GetNoise(THash hash, Point2D point, float frequency = 1)
    {
        var vPoint = point.Value * frequency;

        var latticeX = FastFloor(vPoint.X);
        var latticeY = FastFloor(vPoint.Y);

        var relativeX = vPoint.X - latticeX;
        var relativeY = vPoint.Y - latticeY;

        float firstMinimum = float.MaxValue,
            secondMinimum = float.MaxValue,
            thirdMinimum = float.MaxValue;

        for (int i = -1; i <= 1; i++)
        {
            var latticeDeltaX = relativeX - i;
            var xHash = hash.Eat(latticeX + i);

            for (int j = -1; j <= 1; j++)
            {
                var latticeDeltaY = relativeY - j;

                // Point1 = (features.X, features.Y)
                // Point2 = (features.Z, features.W)
                var features = xHash.Eat(latticeY + j).NextVector4();

                var distance = Metric.Distance(
                    features.X - latticeDeltaX,
                    features.Y - latticeDeltaY
                );
                InsertionSortOfThree(
                    distance,
                    ref firstMinimum,
                    ref secondMinimum,
                    ref thirdMinimum
                );

                distance = Metric.Distance(features.Z - latticeDeltaX, features.W - latticeDeltaY);
                InsertionSortOfThree(
                    distance,
                    ref firstMinimum,
                    ref secondMinimum,
                    ref thirdMinimum
                );
            }
        }

        return Function.Evaluate(
                Metric.Finalize(firstMinimum),
                Metric.Finalize(secondMinimum),
                Metric.Finalize(thirdMinimum)
            ) * 2f
            - 1f;
    }
}
