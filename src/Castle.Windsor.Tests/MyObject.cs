using System.Collections.Generic;

namespace Castle.Windsor.Tests;

public class MyObject : IMyObject
{
    protected readonly IDictionary<int, IList<string>> Stuff;

    public MyObject(IDictionary<int, IList<string>> stuff)
    {
        Stuff = stuff;
    }

    public virtual int Count => Stuff.Count;
}