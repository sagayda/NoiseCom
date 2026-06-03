namespace NoiseCom.LocationSearch;

public interface IValueAggregator
{
    public float Aggregate(ReadOnlySpan<float> values);
}
