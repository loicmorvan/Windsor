using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

[UsedImplicitly]
public class MyObject(IDictionary<int, IList<string>> stuff) : IMyObject
{
    public virtual int Count => stuff.Count;
}