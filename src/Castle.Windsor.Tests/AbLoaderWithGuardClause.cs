using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Resolvers;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

public class AbLoaderWithGuardClause : ILazyComponentLoader
{
    public bool CanLoadNow { get; set; }

    public IRegistration Load(string name, Type service, Arguments arguments)
    {
        Assert.True(CanLoadNow);

        if (service == typeof(A) || service == typeof(B))
        {
            return Component.For(service);
        }

        return null;
    }
}