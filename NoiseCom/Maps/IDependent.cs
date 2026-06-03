namespace NoiseCom.Maps;

public interface IDependent<TPoint> : IMap<TPoint>
    where TPoint : struct, IDimensionalPoint<TPoint>
{
    public IReadOnlyCollection<IMap<TPoint>> Dependencies { get; }
}
