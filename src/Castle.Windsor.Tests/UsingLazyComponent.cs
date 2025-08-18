namespace Castle.Windsor.Tests;

public class UsingLazyComponent(IHasDefaultImplementation dependency)
{
    public IHasDefaultImplementation Dependency { get; } = dependency;
}