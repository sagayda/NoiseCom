namespace NoiseCom.Maps;

public interface IMap<TPoint>
    where TPoint : struct, IDimensionalPoint<TPoint>
{
    public void Evaluate(in GenContext context, TPoint point);
}
