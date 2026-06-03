using NoiseCom.Noise.Hash;

namespace NoiseCom.Noise.Generators;

public class NoiseSampler<THash, TPoint>(INoise<THash, TPoint> noise, int seed = 0)
    where THash : IHash<THash>
    where TPoint : struct, IDimensionalPoint<TPoint>
{
    private SmallXXHash _random = new(seed);

    public int Seed { get; } = seed;
    public INoise<THash, TPoint> Noise { get; } = noise;

    public void Fill(TPoint point, Span<float> destination)
    {
        _random = new(Seed);

        for (int i = 0; i < destination.Length; i++)
            destination[i] = Noise.GetNoise(THash.Seed((int)_random.Eat(i).HashUint()), point);
    }

    public IEnumerable<float> Stream(TPoint point, int? limit = null)
    {
        for (int count = 0; limit == null || count < limit; count++)
        {
            int nextSeed = (int)_random.Eat(count).HashUint();

            yield return Noise.GetNoise(THash.Seed(nextSeed), point);
        }
    }

    public float[] GetBatch(TPoint point, int count)
    {
        float[] results = new float[count];

        Fill(point, results);

        return results;
    }
}
