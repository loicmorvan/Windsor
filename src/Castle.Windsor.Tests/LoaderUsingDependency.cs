using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Resolvers;

namespace Castle.Windsor.Tests;

public class LoaderUsingDependency : ILazyComponentLoader
{
    public IRegistration Load(string name, Type service, Arguments? arguments)
    {
        return Load(service, arguments);
    }

    public IRegistration? Load(string name, Arguments? arguments)
    {
        return null;
    }

    public IRegistration Load(Type service, Arguments? arguments)
    {
        return Component.For(service).DependsOn(arguments);
    }
}