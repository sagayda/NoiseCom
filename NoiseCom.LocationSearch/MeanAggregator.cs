using System.Runtime.CompilerServices;

namespace NoiseCom.LocationSearch;

public readonly struct MeanAggregator : IValueAggregator
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Aggregate(ReadOnlySpan<float> values)
    {
        if (values.IsEmpty)
            return 0f;

        float sum = 0;
        foreach (var v in values)
            sum += v;

        return sum / values.Length;
    }
}
