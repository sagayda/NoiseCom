using System.Runtime.CompilerServices;

namespace NoiseCom.Maps;

// TODO: Add field for map that is the target of the strategy
public sealed class GenStrategySimple<TPoint> : GenStrategyBase<TPoint>
    where TPoint : struct, IDimensionalPoint<TPoint>
{
    private GenStrategySimple(IMap<TPoint>[] order, int[] mappings, int[] offsets)
        : base(order, mappings, offsets) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ExtractResults(Span<float> from)
    {
        // DFS always places the first map (the root of dependency tree) at the end
        return from[^1];
    }

    public static GenStrategySimple<TPoint> CreateFor(IMap<TPoint> map)
    {
        var order = DFS([map]);
        var mappings = new List<int>();
        var offsets = new List<int>();

        int currentOffset = 0;
        for (int i = 0; i < order.Count; i++)
        {
            var currentMap = order[i];
            // write the start offset of the current group
            offsets.Add(currentOffset);

            if (currentMap is IDependent<TPoint> dependent)
            {
                // add all the dependecies
                foreach (var dependency in dependent.Dependencies)
                    mappings.Add(order.FindIndex((item) => item == dependency));

                currentOffset += dependent.Dependencies.Count;
            }

            // add the current map index to the end of the group - the end of the group is always used to write the results
            mappings.Add(i);
            currentOffset++;
        }

        // the last element points to the end of the array
        offsets.Add(currentOffset);

        return new([.. order], [.. mappings], [.. offsets]);
    }
}
