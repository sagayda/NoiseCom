namespace NoiseCom.LocationSearch;

public interface IDimensionalSearchRegion<TPoint>
    where TPoint : struct, IDimensionalPoint<TPoint>
{
    public uint LocationsCount { get; }

    public TPoint[] GetLocations();
}
