namespace NoiseCom.Serialization;

public class NoiseCompositeModel(string hashDefinition, string dimensionDefinition, int rootModelId)
{
    public string HashDefinition { get; set; } = hashDefinition;
    public string DimensionDefinition { get; set; } = dimensionDefinition;

    public int RootModelId { get; set; } = rootModelId;

    public List<NoiseModel> Models { get; set; } = [];
}
