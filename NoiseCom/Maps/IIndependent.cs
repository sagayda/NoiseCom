namespace NoiseCom.Maps;

public interface IIndependent<TPoint> : IMap<TPoint>
    where TPoint : struct, IDimensionalPoint<TPoint>
{
    public float FreeEvaluate(TPoint point);
}
