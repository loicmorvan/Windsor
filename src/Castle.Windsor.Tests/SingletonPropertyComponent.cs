using Castle.Windsor.Core;
using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

[Singleton]
[UsedImplicitly]
public class SingletonPropertyComponent
{
    public static int CtorCallsCount;

    public SingletonPropertyComponent()
    {
        CtorCallsCount++;
    }

    public SingletonPropertyDependency Dependency { get; set; }
}