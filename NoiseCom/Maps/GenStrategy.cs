namespace NoiseCom.Maps;

public abstract class GenStrategyBase<TPoint>
    where TPoint : struct, IDimensionalPoint<TPoint>
{
    protected readonly IMap<TPoint>[] _order;
    protected readonly int[] _mappings;
    protected readonly int[] _offsets;

    public ReadOnlyMemory<IMap<TPoint>> Order => _order;
    public ReadOnlySpan<int> Mappings => _mappings;
    public ReadOnlySpan<int> Offsets => _offsets;

    protected GenStrategyBase(IMap<TPoint>[] order, int[] mappings, int[] offsets)
    {
        _order = order;
        _mappings = mappings;
        _offsets = offsets;
    }

    protected static List<IMap<TPoint>> DFS(IEnumerable<IMap<TPoint>> items)
    {
        Stack<IMap<TPoint>> path = [];
        List<IMap<TPoint>> result = [];
        HashSet<IMap<TPoint>> visited = [],
            processing = [];

        foreach (var item in items)
            Visit(item, visited, processing, result, path);

        return result;
    }

    private static void Visit(
        IMap<TPoint> item,
        HashSet<IMap<TPoint>> visited,
        HashSet<IMap<TPoint>> processing,
        List<IMap<TPoint>> result,
        Stack<IMap<TPoint>> path
    )
    {
        if (visited.Contains(item))
            return;

        if (processing.Add(item) == false)
        {
            var cyclePath = string.Join(" -> ", path.Select((map) => map.GetType().FullName));
            throw new InvalidOperationException(
                $"A depenency cycle was discovered: {cyclePath} -> {item.GetType().FullName}"
            );
        }

        path.Push(item);

        try
        {
            if (item is IDependent<TPoint> dependent)
                foreach (var dep in dependent.Dependencies)
                    Visit(dep, visited, processing, result, path);

            visited.Add(item);
            result.Add(item);
        }
        finally
        {
            processing.Remove(item);
            path.Pop();
        }
    }
}
