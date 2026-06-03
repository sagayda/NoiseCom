using System.Runtime.CompilerServices;

namespace NoiseCom.Noise;

internal static class NoiseHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int FastFloor(float value)
    {
        return value > 0 ? (int)value : (int)value - 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float FastPow(float value, float power)
    {
        return power switch
        {
            1f => value,
            2f => value * value,
            3f => value * value * value,
            4f => value * value * value * value,
            _ => MathF.Pow(value, power),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float Lerp(float a, float b, float t)
    {
        return MathF.FusedMultiplyAdd(t, b - a, a); // t * (b - a) + a => (1 - t) * a + t * b
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void InsertionSortOfThree(
        float value,
        ref float firstMinimum,
        ref float secondMinimum,
        ref float thirdMinimum
    )
    {
        if (value < thirdMinimum)
        {
            if (value < secondMinimum)
            {
                if (value < firstMinimum)
                {
                    thirdMinimum = secondMinimum;
                    secondMinimum = firstMinimum;
                    firstMinimum = value;
                }
                else
                {
                    thirdMinimum = secondMinimum;
                    secondMinimum = value;
                }
            }
            else
            {
                thirdMinimum = value;
            }
        }
    }
}
