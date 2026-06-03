namespace NoiseCom.Noise.Generators.Voronoi;

public interface IVoronoiMetric2D
{
    public float Distance(float deltaX, float deltaY);

    public float Finalize(float distance);
}
