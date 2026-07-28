using NoiseCom.Noise.Hash;

namespace NoiseCom.Noise.Generators;

public interface IAnalyticalNoise<in THash, TPoint> : INoise<THash, TPoint>
    where THash : IHash<THash>
    where TPoint : struct, IDimensionalPoint<TPoint>
{
    public NoiseSample<TPoint> Sample(THash hash, TPoint point, float frequency = 1f);
    public TPoint GetDerivative(THash hash, TPoint point, float frequency = 1f);

    public float MaxGradientMagnitude { get; }
    public float MaxPartialDerivative { get; }
}
