namespace NoiseCom.Serialization;

public class NoiseModel(int id, string typeAlias)
{
    public int Id { get; set; } = id;
    public string TypeAlias { get; set; } = typeAlias;

    public Dictionary<string, object> Parameters { get; set; } = [];
    public Dictionary<string, int> Links { get; set; } = [];
}
