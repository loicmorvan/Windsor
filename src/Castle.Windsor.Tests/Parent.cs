using Castle.Windsor.MicroKernel;

namespace Castle.Windsor.Tests;

public class Parent : List<IChild>, IParent
{
    // ReSharper disable once UnusedParameter.Local
    public Parent(IKernel kernel)
    {
    }
}