using System.Reflection;

namespace Castle.Windsor.Tests;

public static class AssemblyHelper
{
    public static Assembly GetCurrentAssembly()
    {
        return typeof(AbstractContainerTestCase).GetTypeInfo().Assembly;
    }
}