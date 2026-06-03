namespace NoiseCom.Maps;

public sealed class GenStrategyComplex<TPoint> : GenStrategyBase<TPoint>
    where TPoint : struct, IDimensionalPoint<TPoint>
{
    private readonly int[] _requestedIndices;

    private GenStrategyComplex(
        IMap<TPoint>[] order,
        int[] mappings,
        int[] offsets,
        int[] requestedIndices
    )
        : base(order, mappings, offsets)
    {
        _requestedIndices = requestedIndices;
    }

    public void ExtractResults(Span<float> from, Span<float> into)
    {
        ReadOnlySpan<int> indices = _requestedIndices;

        int length = Math.Min(indices.Length, into.Length);

        // TODO: Add sequential copy support
        for (int i = 0; i < length; i++)
            into[i] = from[indices[i]];
    }

    public static GenStrategyComplex<TPoint> CreateFor(IList<IMap<TPoint>> maps)
    {
        var order = DFS(maps);
        var mappings = new List<int>();
        var offsets = new List<int>();
        var requestedIndices = new int[maps.Count]; // we need to save the indices of the requested maps to be able to find their results in the buffer

        int currentRequestedIndex = 0;
        int currentOffset = 0;
        for (int i = 0; i < order.Count; i++)
        {
            var currentMap = order[i];
            // write the start offset of the group
            offsets.Add(currentOffset);

            if (currentMap == maps[currentRequestedIndex])
                requestedIndices[currentRequestedIndex++] = i;

            if (currentMap is IDependent<TPoint> dependent)
            {
                // add all the dependecies
                foreach (var dependency in dependent.Dependencies)
                    mappings.Add(order.FindIndex((map) => map == dependency));

                currentOffset += dependent.Dependencies.Count;
            }

            // add the current map index to the end of the group - the end of the group is always used to write the results
            mappings.Add(i);
            currentOffset++;
        }

        // the last element points to the end of the array
        offsets.Add(currentOffset);

        return new([.. order], [.. mappings], [.. offsets], requestedIndices);
    }
}
