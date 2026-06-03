using System.Runtime.CompilerServices;

namespace NoiseCom;

/// <summary>
/// docs
/// </summary>
public static class Utils
{
    /// <summary>
    /// Remaps <paramref name="value"/> from the range [0; 1] to the range [-1; 1].
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToSigned(float value)
    {
        return (value * 2f) - 1f;
    }

    /// <summary>
    /// Remaps <paramref name="value"/> from the range [-1; 1] to the range [0; 1].
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToUnsigned(float value)
    {
        return (value * 0.5f) + 0.5f;
    }
}
