using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Handlers;

namespace Castle.Windsor.Tests;

internal class DelegatingServiceStrategy(Func<Type, ComponentModel, bool> supports) : IGenericServiceStrategy
{
    public bool Supports(Type service, ComponentModel component)
    {
        return supports.Invoke(service, component);
    }
}