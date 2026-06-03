using System.Runtime.CompilerServices;
using NoiseCom.Serialization;
using static NoiseCom.Noise.Hash.HashHelper;

namespace NoiseCom.Noise.Hash;

[ModelType("Permutation")]
public readonly struct PermutationHash : IHash8<PermutationHash>
{
    private readonly byte[] _permutation;
    private readonly byte _accumulator;

    private PermutationHash(byte[] permutation, byte accumulator)
    {
        _permutation = permutation;
        _accumulator = accumulator;
    }

    public PermutationHash()
    {
        _permutation = [.. _permutation_original, .. _permutation_original];
    }

    public PermutationHash(int seed)
    {
        _permutation = Shuffle(seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PermutationHash Seed(int seed)
    {
        return new(seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PermutationHash Eat(int data)
    {
        return new(_permutation, _permutation[_accumulator + (data & 255)]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PermutationHash Eat(byte data)
    {
        return new(_permutation, _permutation[_accumulator + data]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PermutationHash Shift(int offset)
    {
        return new(_permutation, (byte)(_accumulator + offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte HashByte()
    {
        return _accumulator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextFloat8()
    {
        return ByteToFloat8(_accumulator);
    }

    // csharpier-ignore
    private static readonly byte[] _permutation_original = [
        251, 204,  37, 136, 221, 187, 241, 159, 160,  72,  85,  97, 134, 108, 126, 242,
          2, 192, 218, 183, 110,  11, 216, 243, 157,  51, 153, 240,   9, 128, 164,  39,
        158,  66, 202,  68, 255, 102, 233,  86, 171,  67,  30,  53, 176, 226,  76,  59,
        222, 175, 181,  91, 123,  14,  65, 178, 109,  19,  28,  92, 172, 235,  48,  61,
         81,  36, 231, 232,  62, 173, 144, 120, 133, 113,  90, 101,  99,  87, 207, 220,
        230, 143,  27, 107, 167, 245,  42, 206, 151, 253,  38, 130,  50,  95, 177, 100,
         74, 137,  33, 145,   8, 197, 249, 213, 237, 105, 131, 196,  46, 165, 132, 205,
         21, 140,  55, 239,  32, 146, 186, 116,  89, 198,  10,  70,  45, 201, 209, 252,
        114, 229, 254,  73,  71,  83,  88,  23, 227, 103,  64, 180, 236,  49, 234,  82,
        129,   6, 152, 208, 148,  94, 135,  12, 170, 166,  52, 246,  96, 219, 195,  60,
        147,  56, 141, 189, 217,  26, 154,  69, 238,  43,  79,  77,  25,  24,  80,   5,
        127, 117,   0, 214, 179,  44,  40,  29, 248,  93, 119,  47,   7, 223,  84,  20,
        193, 150, 184, 215, 121,   4, 162, 111, 118,  78,  13, 115,  98, 224,  16, 156,
        163, 244,  15,  34, 124, 199,   1, 211,  57, 112, 188,   3, 149,  31, 194, 185,
        155,  35,  18, 182, 168, 142, 161, 106, 250,  22, 191, 225, 174, 138, 139, 169,
         75, 125, 210, 104, 212, 122, 228,  41, 247,  58,  17,  63, 190, 203, 200,  54
    ];

    private static byte[] Shuffle(int seed)
    {
        // HACK: Random may be inconsistent across versions & platforms
        var result = new byte[512];

        var source = new Span<byte>(result, 0, 256);
        _permutation_original.CopyTo(source);

        Random rnd = new(seed);
        rnd.Shuffle(source);

        source.CopyTo(new Span<byte>(result, 256, 256));
        return result;
    }
}
