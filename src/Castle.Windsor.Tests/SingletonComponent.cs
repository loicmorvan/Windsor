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

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public SingletonDependency Dependency { get; set; }
}