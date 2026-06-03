using System.Runtime.CompilerServices;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Ridged;

public readonly struct RidgedSettings
{
    /// <summary>
    /// Gets a value indicating whether the final noise output should be remapped from <c>[0, 1]</c> to <c>[-1, 1]</c>.
    /// <para>
    /// Most standard noise generators output values in the <c>[-1, 1]</c> range.
    /// Enabling this ensures consistency when blending ridged noise with other noise types.
    /// Disabling it keeps the raw positive values, which can be useful for specific masking or multiplication operations.
    /// </para>
    /// </summary>
    [ModelProperty]
    public bool Normalize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init;
    }

    /// <summary>
    /// Gets the exponent applied to the absolute noise value.
    /// <para>
    /// Mathematically represented as <c>pow(abs(noise), Power)</c>.
    /// Values greater than 1 sharpen the ridges, making valleys wider and peaks narrower.
    /// Values between 0 and 1 soften the transitions, making the ridges less pronounced.
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

    /// <summary>
    /// Gets the inversion strategy applied to the ridged noise.
    /// <para>
    /// Determines at which stage the noise is inverted. This fundamentally changes the visual structure,
    /// allowing you to choose between protruding ridges or deep, sharp crevices.
    /// </para>
    /// </summary>
    [ModelProperty]
    public RidgedInversion Inversion
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init;
    }

    public static readonly RidgedSettings Default = new()
    {
        Normalize = true,
        Power = 1f,
        Inversion = RidgedInversion.Invert,
    };
}
