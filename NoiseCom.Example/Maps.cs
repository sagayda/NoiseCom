using NoiseCom.Maps;
using NoiseCom.Noise.Generators.Fractal;
using NoiseCom.Noise.Generators.Perlin;
using NoiseCom.Noise.Generators.Ridged;
using NoiseCom.Noise.Generators.Simplex;
using NoiseCom.Noise.Generators.Warped;
using NoiseCom.Noise.Gradients.TwoDimensional;
using NoiseCom.Noise.Hash;
using static NoiseCom.Utils;

namespace NoiseCom.Example;

// Default fBm noise for the terrain base
public class ElevationMap : IMap<Point2D> // You can also use IIndependent interface to be able to evaluate the map without GenContext (outside Puff)
{
    private readonly MurMur3Hash _hash;
    private readonly Fractal<
        MurMur3Hash,
        Point2D,
        Simplex2D<MurMur3Hash, CircularWeighted<MurMur3Hash>> // CircularWeighted is the most expensive and highest quality gradient.
    > _fractalPerlin; // Weighted means the gradients can have lengths other than 1.
    private readonly float _frequency;

    public ElevationMap(float frequency = 0.01f)
    {
        _hash = new();
        _fractalPerlin = new(new(), FractalSettings.Default with { Octaves = 2 });
        _frequency = frequency;
    }

    public void Evaluate(in GenContext context, Point2D point)
    {
        // Get raw noise [-1, 1]
        float height = _fractalPerlin.GetNoise(_hash, point, _frequency);

        // Normalize to slightly flattened plains with peaks
        // Mapping [-1, 1] -> [0, 1] with emphasis on lower ground
        height = ToUnsigned(height);
        height = MathF.Pow(height, 1.5f);
        height = ToSigned(height);

        context.Result = height;
    }
}

// Ridged Simplex noise to simulate tectonic plates & mountain ranges
public class TectonicRiftMap : IMap<Point2D>
{
    private readonly Ridged<
        MurMur3Hash,
        Point2D,
        Simplex2D<MurMur3Hash, SquareUnitNormalized<MurMur3Hash>> // SquareUnit is a pretty cheap gradient, but we don't need much details for this map
    > _ridgedSimplex; // Normalized means all the gradients will have their length equal to 1.
    private readonly float _frequency;

    public TectonicRiftMap(float frequency = 0.002f)
    {
        _ridgedSimplex = new(
            new(),
            RidgedSettings.Default with
            {
                Normalize = false,
                Power = 0.5f,
            }
        );
        _frequency = frequency;
    }

    public void Evaluate(in GenContext context, Point2D point)
    {
        // Use ridged noise to create mountain ridges
        context.Result = _ridgedSimplex.GetNoise(default, point, _frequency);
    }
}

// The final terrain map.
// Combination of Elevation and Rift maps.
public class CombinedTerrainMap : IDependent<Point2D>
{
    public IReadOnlyCollection<IMap<Point2D>> Dependencies { get; }

    public CombinedTerrainMap(ElevationMap elevation, TectonicRiftMap rifts)
    {
        Dependencies = [elevation, rifts]; // Specifying dependencies allows then to retrieve their results from GenContext.
    }

    public void Evaluate(in GenContext context, Point2D point)
    {
        float baseHeight = context.GetValue(0); // the index here is the index from the Dependencies collection
        float riftFactor = context.GetValue(1); // [0, 1] from ridged, because we have RidgedSettings.Normalize = false in TectonicRiftMap

        float finalHeight = baseHeight;

        // If riftFactor is high (sharp ridge), we grows the mountain
        const float minimumRift = 0.6f;
        if (riftFactor > minimumRift)
        {
            const float intensity = 2.5f;
            // Linear interpolation to carve out the terrain
            float depth = (riftFactor - minimumRift) * intensity;
            finalHeight += depth;
        }

        context.Result = finalHeight;
    }
}

// Warped noise to simulate wind
public class WindFieldMap : IMap<Point2D>
{
    private readonly Warped2D<
        MurMur3Hash,
        Simplex2D<MurMur3Hash, OctahedralWeighted<MurMur3Hash>>, // Good gradient for the base noise
        Perlin2D<MurMur3Hash, SquareUnitNormalized<MurMur3Hash>> // Cheap one for the warp noise
    > _warpedNoise;
    private readonly float _frequency;

    public WindFieldMap(float freq = 0.005f)
    {
        _warpedNoise = new(new(), new(), WarpedSettings.Default with { Warps = 2, Power = 1.5f });
        _frequency = freq;
    }

    public void Evaluate(in GenContext context, Point2D point)
    {
        // Just retur the value
        context.Result = _warpedNoise.GetNoise(default, point, _frequency);
    }
}

// Temperature map dependes on the terrain: the heigher the spot - the lower the temperature
public class TemperatureMap : IDependent<Point2D>
{
    public IReadOnlyCollection<IMap<Point2D>> Dependencies { get; }
    private readonly Simplex2D<SmallXXHash, OctahedralWeighted<SmallXXHash>> _simplexNoise;

    public TemperatureMap(CombinedTerrainMap terrain)
    {
        Dependencies = [terrain];
        _simplexNoise = new();
    }

    public void Evaluate(in GenContext context, Point2D point)
    {
        float height = ToUnsigned(context.GetValue(0));

        // Base noise for temperature fluctuations (seasons/latitude simulation)
        float baseTempNoise = _simplexNoise.GetNoise(default, point, 0.003f); // We use a low frequency here. This is similar to map scaling.
        // You will probably want proportional frequency within dependent maps.
        // Adiabatic lapse rate simulation: Higher = Colder
        float heightPenalty = height * height * height;

        context.Result = baseTempNoise - heightPenalty;
    }
}

// Simulates moisture of the terrain
public class MoistureMap : IDependent<Point2D>
{
    public IReadOnlyCollection<IMap<Point2D>> Dependencies { get; }
    private readonly Perlin2D<SmallXXHash, OctahedralWeighted<SmallXXHash>> _perlinNoise;

    public MoistureMap(CombinedTerrainMap terrain, TemperatureMap temp, WindFieldMap wind)
    {
        Dependencies = [terrain, temp, wind];
        _perlinNoise = new();
    }

    public void Evaluate(in GenContext context, Point2D point)
    {
        float height = ToUnsigned(context.GetValue(0));
        float temp = context.GetValue(1);
        float windTurbulence = context.GetValue(2);

        // Base moisture noise
        float globalMoisture = _perlinNoise.GetNoise(default, point, 0.008f);

        // Hotter areas dry out faster (Temp > 0 reduces moisture)
        if (temp > 0f)
            globalMoisture -= temp * 0.5f;

        // Wind turbulence distributes moisture chaotically
        globalMoisture -= windTurbulence * 0.2f;

        // Simple "Coastline" logic: Low height implies water proximity
        float heightImpact = 0.3f - Math.Min(height * height, 0.3f);
        globalMoisture += heightImpact * 2f;

        context.Result = Math.Clamp(globalMoisture, -1f, 1f);
    }
}
