using System.Runtime.CompilerServices;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Warped;

public readonly struct WarpedSettings
{
    /// <summary>
    /// Gets the number of recursion iterations applied to the domain warping.
    /// <para>
    /// A value of 1 represents a standard single-step warp.
    /// Higher values apply the warping recursively (e.g., <c>noise(p + noise(p + ...))</c>),
    /// creating more complex, fluid-like, or marble-like patterns at the cost of performance.
    /// </para>
    /// </summary>
    [ModelProperty]
    public int Warps
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init;
    }

    /// <summary>
    /// Gets the intensity multiplier for the coordinate displacement.
    /// <para>
    /// This value determines how far the coordinates are shifted.
    /// Higher values result in more severe distortion and stretching of the noise features.
    /// Typically, this should be balanced with the noise frequency to maintain visual consistency.
    /// </para>
    /// </summary>
    [ModelProperty]
    public float Power
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init;
    }

    public static readonly WarpedSettings Default = new() { Warps = 1, Power = 1f };
}
