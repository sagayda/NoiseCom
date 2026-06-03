namespace NoiseCom.LocationSearch;

public class PerDimensionSquareSearchRegion2D(
    Point2D position,
    float sizePerDimension,
    uint locationsPerDimension
) : PerDimensionSearchRegion<Point2D>(position, sizePerDimension, locationsPerDimension)
{
    public override uint LocationsCount { get; } = locationsPerDimension * locationsPerDimension;

    public override Point2D[] GetLocations()
    {
        float step = SizePerDimension / LocationsPerDimension;
        float initialOffset = step * 0.5f;

        Point2D xStep = new(step, 0);
        Point2D yStep = new(0, step);

        Point2D[] locations = new Point2D[LocationsCount];

        Point2D currentX = new(Position.X + initialOffset, Position.Y + initialOffset);
        for (int xi = 0; xi < LocationsPerDimension; xi++)
        {
            var currentY = currentX;
            for (int yi = 0; yi < LocationsPerDimension; yi++)
            {
                locations[(xi * LocationsPerDimension) + yi] = currentY;

                currentY += yStep;
            }
            currentX += xStep;
        }

        return locations;
    }
}

public class PerDimensionSquareSearchRegion1D(
    Point1D position,
    float sizePerDimension,
    uint locationsPerDimension
) : PerDimensionSearchRegion<Point1D>(position, sizePerDimension, locationsPerDimension)
{
    public override uint LocationsCount { get; } = locationsPerDimension;

    public override Point1D[] GetLocations()
    {
        float step = SizePerDimension / LocationsPerDimension;
        float initialOffset = step * 0.5f;

        Point1D xStep = new(step);

        Point1D[] locations = new Point1D[LocationsPerDimension];

        Point1D currentX = new(Position.X + initialOffset);
        for (int xi = 0; xi < LocationsPerDimension; xi++)
        {
            locations[xi] = currentX;

            currentX += xStep;
        }

        return locations;
    }
}

public abstract class PerDimensionSearchRegion<TPoint>(
    TPoint position,
    float sizePerDimension,
    uint locationsPerDimension
) : IDimensionalSearchRegion<TPoint>
    where TPoint : struct, IDimensionalPoint<TPoint>
{
    public TPoint Position { get; } = position;

    public float SizePerDimension { get; } = sizePerDimension;
    public uint LocationsPerDimension { get; } = locationsPerDimension;

    public abstract uint LocationsCount { get; }

    public abstract TPoint[] GetLocations();
}
