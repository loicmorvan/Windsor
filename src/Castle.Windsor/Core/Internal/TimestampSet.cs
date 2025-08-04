namespace Castle.Windsor.Core.Internal;

/// <summary>Holds a timestamp (integer) for a given item</summary>
internal class TimestampSet
{
    private readonly Dictionary<IVertex, int> _items = new();

    public void Register(IVertex item, int time)
    {
        _items[item] = time;
    }

    public int TimeOf(IVertex item)
    {
        return _items[item];
    }
}