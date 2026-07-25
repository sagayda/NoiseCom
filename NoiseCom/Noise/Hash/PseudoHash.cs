namespace NoiseCom.Noise.Hash;

public readonly struct PseudoHash : IHash8<PseudoHash>
{
    private readonly List<int> _history = [];

    public PseudoHash() { }

    private PseudoHash(PseudoHash original)
    {
        foreach (var item in original._history)
        {
            _history.Add(item);
        }
    }

    public static PseudoHash Seed(int seed)
    {
        throw new NotImplementedException();
    }

    public PseudoHash Eat(int data)
    {
        // _history.Add(data);
        var newHash = new PseudoHash(this);
        newHash._history.Add(data);

        return newHash;
    }

    public int GetData(int position)
    {
        return _history[position];
    }

    public byte HashByte()
    {
        throw new NotImplementedException();
    }

    public float NextFloat8()
    {
        throw new NotImplementedException();
    }

    public PseudoHash Shift(int offset)
    {
        return this;
    }
}
