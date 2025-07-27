using System.Collections.Generic;

namespace Castle.Windsor.Core.Internal;

/// <summary>Holds a timestamp (integer) for a given item</summary>
internal class TimestampSet
{
    private readonly IDictionary<IVertex, int> _items = new Dictionary<IVertex, int>();

    public void Register(IVertex item, int time)
    {
        _items[item] = time;
    }

    public int TimeOf(IVertex item)
    {
        return _items[item];
    }
}