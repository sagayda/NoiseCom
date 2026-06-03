namespace NoiseCom.Noise.Generators.Perlin;

using static NoiseCom.Noise.NoiseHelper;

public readonly struct LatticeSpan
{
    public readonly int Floor,
        Ceil; // X, X + 1
    public readonly float DeltaFloor,
        DeltaCeil; // distances to X and X + 1
    public readonly float Fade,
        DFade; // fade(point - X)

    public LatticeSpan(float point)
    {
        Floor = FastFloor(point);
        Ceil = Floor + 1;

        DeltaFloor = point - Floor;
        DeltaCeil = DeltaFloor - 1f;

        float t = DeltaFloor;
        float tSqr = t * t;
        float tCub = tSqr * t;

        // TODO: DI
        Fade = tCub * (t * (t * 6f - 15f) + 10f);
        DFade = tSqr * (t * (30f * t - 60f) + 30f);
    }
}
