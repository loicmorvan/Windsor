using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

public class ClassWithServiceDependency
{
    public ClassWithServiceDependency(IService dependency)
    {
    }
}