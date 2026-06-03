namespace NoiseCom.LocationSearch;

public class HybridAHPLocationSearch<TAggregator>
    where TAggregator : IValueAggregator
{
    private const int MaxStackAllocSize = 256;

    // csharpier-ignore
    private static readonly float[] RandomIndices =
    [
        0f, 0f, 0f, 0.58f, 0.90f, 1.12f, 1.24f, 1.32f, 1.41f, 1.45f, 1.49f, 1.51f, 1.48f, 1.56f, 1.57f, 1.59f,
    ];

    public TAggregator Aggregator { get; }

    public HybridAHPLocationSearch(TAggregator aggregator)
    {
        Aggregator = aggregator;
    }

    public List<float> Find<TPoint>(
        ReadOnlySpan<IDimensionalSearchRegion<TPoint>> regions,
        ReadOnlySpan<ICriteria<TPoint>> criterias,
        float[,] criteriaComparisonMatrix
    )
        where TPoint : struct, IDimensionalPoint<TPoint>
    {
        var regionsCount = regions.Length;
        var criteriasCount = criterias.Length;

        // Copy the matrix to avoid mutating the original array passed by the caller
        var criteriaMatrix = new float[criteriasCount, criteriasCount];
        Array.Copy(criteriaComparisonMatrix, criteriaMatrix, criteriaComparisonMatrix.Length);

        var criteriaWeights = EnsureConsistency(criteriaMatrix);

        // Build the score matrix (Rows - alternatives, Columns - criteria)
        var scoreMatrix = new float[regionsCount, criteriasCount];
        for (int regionI = 0; regionI < regionsCount; regionI++)
        {
            var locations = regions[regionI].GetLocations();
            var locationsCount = locations.Length;

            Span<float> locationWeights =
                locationsCount > MaxStackAllocSize
                    ? new float[locationsCount]
                    : stackalloc float[locationsCount];

            for (int criteriaI = 0; criteriaI < criteriasCount; criteriaI++)
            {
                for (int locationI = 0; locationI < locationsCount; locationI++)
                    locationWeights[locationI] = criterias[criteriaI].Weight(locations[locationI]);

                scoreMatrix[regionI, criteriaI] = Aggregator.Aggregate(locationWeights);
            }
        }

        // Multiply the score matrix by the criteria weights
        var globalWeights = new List<float>(regionsCount);
        for (int i = 0; i < regionsCount; i++)
        {
            float sum = 0f;
            for (int j = 0; j < criteriasCount; j++)
            {
                sum += scoreMatrix[i, j] * criteriaWeights[j];
            }
            globalWeights.Add(sum);
        }

        return globalWeights;
    }

    private float[] EnsureConsistency(float[,] matrix)
    {
        var weights = CalculateWeights(matrix, out float cr);
        int n = matrix.GetLength(0);

        if (cr <= 0.1f || n <= 2)
            return weights;

        // Rebuild matrix for perfect consistency if CR is unacceptable
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                    matrix[i, j] = 1f;
                else
                    matrix[i, j] = weights[i] / weights[j];
            }
        }

        return CalculateWeights(matrix, out _);
    }

    private float[] CalculateWeights(float[,] comparisonMatrix, out float cr)
    {
        int n = comparisonMatrix.GetLength(0);
        float[] weights = new float[n];
        float[] nextWeights = new float[n];

        for (int i = 0; i < n; i++)
            weights[i] = 1f / n;

        // TODO: public props?
        const int maxIterations = 100;
        const float tolerance = 1e-6f;

        float lambdaMax = 0f;

        // Power iteration method to find principal eigenvector and maximum eigenvalue
        for (int iter = 0; iter < maxIterations; iter++)
        {
            lambdaMax = 0f;

            // comparisonMatrix * weights
            for (int i = 0; i < n; i++)
            {
                float sum = 0f;
                for (int j = 0; j < n; j++)
                    sum += comparisonMatrix[i, j] * weights[j];

                nextWeights[i] = sum;
                lambdaMax += sum;
            }

            float diff = 0f;
            for (int i = 0; i < n; i++)
            {
                nextWeights[i] /= lambdaMax;
                diff += MathF.Abs(nextWeights[i] - weights[i]);
                weights[i] = nextWeights[i];
            }

            if (diff < tolerance)
                break;
        }

        float ci = (lambdaMax - n) / (n - 1);
        float ri = n < RandomIndices.Length ? RandomIndices[n] : 1.59f;
        cr = n > 2 ? ci / ri : 0f;

        return weights;
    }
}
