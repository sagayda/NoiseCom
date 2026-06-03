using System.Numerics;
using NoiseCom.Noise.Hash;

namespace NoiseCom.Noise.Gradients.TwoDimensional;

public interface IAnalyticalGradient2D<THash> : IGradient2D<THash>
    where THash : IHash8<THash>
{
    public (float Value, Vector2 Derivatives) EvaluateCombined(THash hash, float x, float y);
    public (float Value, float Dx, float Dy) EvaluateCombinedScalar(THash hash, float x, float y);

    public Vector2 EvaluateDerivatives(THash hash, float x, float y);
    public (float Dx, float Dy) EvaluateDerivativesScalar(THash hash, float x, float y);
}
