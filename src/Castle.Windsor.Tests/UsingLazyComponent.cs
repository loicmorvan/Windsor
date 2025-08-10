namespace Castle.Windsor.Tests;

public class UsingLazyComponent
{
    public UsingLazyComponent(IHasDefaultImplementation dependency)
    {
        Dependency = dependency;
    }

    public IHasDefaultImplementation Dependency { get; }
}