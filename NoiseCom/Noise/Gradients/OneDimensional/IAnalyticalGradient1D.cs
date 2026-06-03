using NoiseCom.Noise.Hash;

namespace NoiseCom.Noise.Gradients.OneDimensional;

public interface IAnalyticalGradient1D<THash> : IGradient1D<THash>
    where THash : IHash8<THash>
{
    public (float Value, float Dx) EvaluateCombined(THash hash, float x);

    public float EvaluateDerivatives(THash hash, float x);
}
