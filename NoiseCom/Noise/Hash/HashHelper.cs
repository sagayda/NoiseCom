using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseCom.Noise.Hash;

internal static class HashHelper
{
    private const float Int24ToFloat01 = 1f / 0xFFFFFF;
    private const float ByteToFloat01 = 1f / 0xFF;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float ByteToFloat8(byte value) => value * ByteToFloat01;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float UintToFloat24(uint value) => (value >> 8) * Int24ToFloat01;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float UintToFloat8(uint value) => (value & 0xff) * ByteToFloat01;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector4 UintToFloat8x4(uint value)
    {
        return new(
            (value & 0xff) * ByteToFloat01,
            ((value >> 8) & 0xff) * ByteToFloat01,
            ((value >> 16) & 0xff) * ByteToFloat01,
            (value >> 24) * ByteToFloat01
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void UintToFloat8x4(
        uint value,
        out float float1,
        out float float2,
        out float float3,
        out float float4
    )
    {
        float1 = (value & 0xff) * ByteToFloat01;
        float2 = ((value >> 8) & 0xff) * ByteToFloat01;
        float3 = ((value >> 16) & 0xff) * ByteToFloat01;
        float4 = (value >> 24) * ByteToFloat01;
    }
}
