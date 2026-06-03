namespace NoiseCom.Noise.Generators.Voronoi;

public interface IVoronoiMetric1D
{
    public float Distance(float deltaX);

    public float Finalize(float distance);
}
