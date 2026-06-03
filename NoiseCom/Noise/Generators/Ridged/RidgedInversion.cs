namespace NoiseCom.Noise.Generators.Ridged;

public enum RidgedInversion
{
    /// <summary>
    /// The absolute noise value is kept as is.
    /// This creates sharp peaks (ridges) and wide, smooth valleys.
    /// </summary>
    NoInvert,

    /// <summary>
    /// The absolute noise is inverted (<c>1.0 - abs(noise)</c>) before the power function is applied.
    /// This flips the terrain, turning sharp ridges into narrow, sharp crevices.
    /// </summary>
    Invert,

    /// <summary>
    /// The noise is inverted after the power exponent has been applied (<c>1.0 - pow(abs(noise), Power)</c>).
    /// This preserves the specific width modifications made by the power function while inverting the final visual output.
    /// </summary>
    InvertAfterPower,
}
