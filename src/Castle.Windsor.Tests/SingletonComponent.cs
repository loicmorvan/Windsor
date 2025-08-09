using Castle.Windsor.Core;
using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

[Singleton]
[UsedImplicitly]
public class SingletonComponent
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public SingletonDependency Dependency { get; set; }
}