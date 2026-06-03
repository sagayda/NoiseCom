using NoiseCom.Noise.Hash;

namespace NoiseCom.Noise.Generators;

public interface INoise<in THash, TPoint>
    where THash : IHash<THash>
    where TPoint : struct, IDimensionalPoint<TPoint>
{
    public float GetNoise(THash hash, TPoint point, float frequency = 1f);
}
