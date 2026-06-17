using NoiseCom.Noise.Hash;

namespace NoiseCom.Noise.Gradients.OneDimensional;

public interface IGradient1D<THash>
    where THash : IHash8<THash>
{
    public float Evaluate(THash hash, float x);
    public float GetGradient(THash hash);
}
