using NoiseCom.Maps;

namespace NoiseCom.Example;

internal static class Program
{
    private static void Main(string[] args)
    {
        // Initialize all the maps
        var elevation = new ElevationMap();
        var rifts = new TectonicRiftMap();
        var terrain = new CombinedTerrainMap(elevation, rifts); // inject the dependencies

        var wind = new WindFieldMap();
        var temperature = new TemperatureMap(terrain);
        var moisture = new MoistureMap(terrain, temperature, wind);

        Point2D point = new(0.123f, -3.21f);
        // If you want a result of the only map, use GenStrategySimple
        var terrainStrat = GenStrategySimple<Point2D>.CreateFor(terrain);
        // Strategies are designed to be created once and reused.
        // Relatively heavy operations are performed during theirs construction.

        var height = Puff.EvaluateStrategy(terrainStrat, point); // Use Puff to evaluate the strategies
        Console.WriteLine($"Height at {point.Value}: {height:F4}");

        // If you want a result of several maps, use GenStrategyComplex.
        // This is superior than using several simple strategies.
        var terrainTemperatureStrat = GenStrategyComplex<Point2D>.CreateFor([
            terrain,
            temperature,
            moisture, // Note that the dependency Wind map will be evaluated under the hood, as well as the Elevation and Rift maps.
        ]);

        var buffer = new float[3];
        Puff.EvaluateStrategy(terrainTemperatureStrat, point, buffer);
        Console.WriteLine(
            $"Height at {point.Value}: {buffer[0]:F4}  Temperature at {point.Value}: {buffer[1]:F4}  Moisture at {point.Value}: {buffer[2]:F4}"
        );
    }
}
