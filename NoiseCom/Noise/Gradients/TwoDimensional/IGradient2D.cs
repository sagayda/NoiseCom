using NoiseCom.Noise.Hash;

namespace NoiseCom.Noise.Gradients.TwoDimensional;

public interface IGradient2D<THash>
    where THash : IHash8<THash>
{
    public float Evaluate(THash hash, float x, float y);
}
