namespace NoiseCom.LocationSearch;

public interface ICriteria<TPoint>
    where TPoint : struct, IDimensionalPoint<TPoint>
{
    public float Weight(TPoint point);
}
