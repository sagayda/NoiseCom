using System.Runtime.CompilerServices;

namespace NoiseCom.Maps;

public readonly ref struct GenContext
{
    private readonly Span<float> _buffer;
    private readonly ReadOnlySpan<int> _mappings;

    public readonly int Length => _mappings.Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GenContext(Span<float> buffer, ReadOnlySpan<int> mappings)
    {
        _buffer = buffer;
        _mappings = mappings;
    }

    public readonly float this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer[_mappings[index]];
    }

    public readonly float Result
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer[_mappings[^1]];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _buffer[_mappings[^1]] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float GetValue(int index)
    {
        return this[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GenContext CreateAtState(
        int state,
        Span<float> buffer,
        ReadOnlySpan<int> mappings,
        ReadOnlySpan<int> offsets
    )
    {
        var start = offsets[state];
        var size = offsets[state + 1] - start;

        return new(buffer, mappings.Slice(start, size));
    }
}
