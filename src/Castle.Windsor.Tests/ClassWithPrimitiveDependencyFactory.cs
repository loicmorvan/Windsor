namespace Castle.Windsor.Tests;

public class ClassWithPrimitiveDependencyFactory
{
    public ClassWithPrimitiveDependency Create()
    {
        return new ClassWithPrimitiveDependency(2);
    }
}