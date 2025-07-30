using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Context;
using Castle.Windsor.MicroKernel.Handlers;

namespace Castle.Windsor.Tests.TestImplementationsOfExtensionPoints;

public class UseStringGenericStrategy : IGenericImplementationMatchingStrategy
{
    public Type[] GetGenericArguments(ComponentModel model, CreationContext context)
    {
        return [typeof(string)];
    }
}