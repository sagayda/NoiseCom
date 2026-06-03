using System.Runtime.CompilerServices;

namespace NoiseCom.Maps;

public static class Puff
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float EvaluateStrategy<TPoint>(GenStrategySimple<TPoint> strategy, TPoint point)
        where TPoint : struct, IDimensionalPoint<TPoint>
    {
        var order = strategy.Order.Span;
        var mappings = strategy.Mappings;
        var offsets = strategy.Offsets;
        Span<float> buffer = stackalloc float[order.Length];

        for (int map = 0; map < order.Length; map++)
        {
            var context = GenContext.CreateAtState(map, buffer, mappings, offsets);
            order[map].Evaluate(in context, point);
        }

        return strategy.ExtractResults(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EvaluateStrategy<TPoint>(
        GenStrategyComplex<TPoint> strategy,
        TPoint point,
        Span<float> destination
    )
        where TPoint : struct, IDimensionalPoint<TPoint>
    {
        var order = strategy.Order.Span;
        var mappings = strategy.Mappings;
        var offsets = strategy.Offsets;
        Span<float> buffer = stackalloc float[order.Length];

        for (int map = 0; map < order.Length; map++)
        {
            var context = GenContext.CreateAtState(map, buffer, mappings, offsets);
            order[map].Evaluate(in context, point);
        }

        strategy.ExtractResults(buffer, destination);
    }
}
