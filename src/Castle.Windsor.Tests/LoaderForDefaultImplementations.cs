using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Resolvers;

namespace Castle.Windsor.Tests;

public class LoaderForDefaultImplementations : ILazyComponentLoader
{
    public IRegistration Load(string name, Type service, Arguments arguments)
    {
        if (!service.GetTypeInfo().IsDefined(typeof(DefaultImplementationAttribute)))
        {
            return null;
        }

        var attributes = service.GetTypeInfo().GetCustomAttributes(typeof(DefaultImplementationAttribute), false);
        var attribute = attributes.First() as DefaultImplementationAttribute;
        Debug.Assert(attribute != null);
        return Component.For(service).ImplementedBy(attribute.Implementation).Named(name);
    }
}