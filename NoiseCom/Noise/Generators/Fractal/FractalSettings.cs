using System.Runtime.CompilerServices;
using NoiseCom.Serialization;

namespace NoiseCom.Noise.Generators.Fractal;

public readonly struct FractalSettings
{
    /// <summary>
    /// Gets the number of noise layers (octaves) combined to form the fractal.
    /// <para>
    /// A value of 1 produces a simple, smooth noise. Higher values add finer details and
    /// high-frequency noise layers, resulting in more natural, terrain-like textures
    /// at the cost of increased computation time.
    /// </para>
    /// <para>
    /// The default value is <c>4</c>.
    /// </para>
    /// </summary>
    [ModelProperty]
    public int Octaves
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init;
    }

    /// <summary>
    /// Gets the frequency multiplier between successive octaves.
    /// <para>
    /// Determines how quickly the details shrink in size for each subsequent layer.
    /// A typical value of 2.0 means each octave is twice as detailed (half the size)
    /// as the previous one.
    /// </para>
    /// <para>
    /// The default value is <c>2.0f</c>.
    /// </para>
    /// </summary>
    [ModelProperty]
    public float Lacunarity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init;
    }

    /// <summary>
    /// Gets the amplitude multiplier between successive octaves.
    /// <para>
    /// Determines the visual weight or influence of each subsequent layer.
    /// A typical value of 0.5 means each octave contributes half as much as the previous one,
    /// ensuring that macro-features dominate the shape while micro-features add subtle surface texture.
    /// </para>
    /// <para>
    /// The default value is <c>0.5f</c>.
    /// </para>
    /// </summary>
    [ModelProperty]
    public float Persistence
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init;
    }

    public static readonly FractalSettings Default = new()
    {
        Octaves = 4,
        Lacunarity = 2,
        Persistence = 0.5f,
    };
}
