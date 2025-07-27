using Castle.Windsor.Core;
using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

[Singleton]
[UsedImplicitly]
public class SingletonComponent
{
    public static int CtorCallsCount;

    public SingletonComponent()
    {
        CtorCallsCount++;
    }

    public SingletonDependency Dependency { get; set; }
}