using System;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Resolvers;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

#pragma warning disable CS9113 // Parameter is unread.
public class LoaderWithDependency(IEmployee employee) : ILazyComponentLoader
#pragma warning restore CS9113 // Parameter is unread.
{
    public IRegistration Load(string name, Type service, Arguments arguments)
    {
        return null;
    }
}