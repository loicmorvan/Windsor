using Castle.Windsor.Core;
using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

[Singleton]
[UsedImplicitly]
public class SingletonDependency
{
    public SingletonDependency(SingletonComponent c)
    {
    }
}