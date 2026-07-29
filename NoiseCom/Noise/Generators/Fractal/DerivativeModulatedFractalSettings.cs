using System.Runtime.CompilerServices;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Fractal;

public readonly struct DerivativeModulatedFractalSettings
{
    /// <inheritdoc cref="FractalSettings.Octaves"/>
    [ModelProperty]
    public int Octaves
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init;
    }

    /// <inheritdoc cref="FractalSettings.Lacunarity"/>
    [ModelProperty]
    public float Lacunarity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init;
    }

    /// <inheritdoc cref="FractalSettings.Persistence"/>
    [ModelProperty]
    public float Persistence
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init;
    }

    /// <summary>
    /// Gets the threshold used to normalize the steepness (derivative magnitude) of the noise.
    /// <para>
    /// Because noise gradients rarely reach their absolute theoretical maximums, this parameter acts as a statistical ceiling.
    /// For example, a value of 0.8 stretches the most statistically common steep slopes to a full 1.0 weight, clipping the rare extreme peaks.
    /// Lowering this value increases the sensitivity of the modulation, causing the maximum detail weight to be reached on gentler slopes.
    /// </para>
    /// <para>
    /// The default value is <c>0.8f</c>.
    /// </para>
    /// </summary>
    [ModelProperty]
    public float EffectiveMaximum
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init;
    }

    /// <summary>
    /// Gets a value indicating whether the detail distribution should be inverted.
    /// <para>
    /// When <c>false</c>, finer details (subsequent octaves) are concentrated on steep slopes, leaving flat plains and valleys relatively smooth (e.g., simulating rocky cliffs and sandy basins).
    /// When <c>true</c>, the effect is reversed: finer details accumulate on flat areas, while steep slopes are heavily smoothed (e.g., simulating severe water erosion on slopes or snow buildup on plains).
    /// </para>
    /// <para>
    /// The default value is <c>false</c>.
    /// </para>
    /// </summary>
    [ModelProperty]
    public bool Invert
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init;
    }

    public static readonly DerivativeModulatedFractalSettings Default = new()
    {
        Octaves = 4,
        Lacunarity = 2,
        Persistence = 0.5f,
        EffectiveMaximum = 0.8f,
        Invert = false,
    };
}
