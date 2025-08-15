using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Handlers;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

public class ReturnAExtension(A a, bool proceed = false) : IResolveExtension
{
    public void Init(IKernel kernel)
    {
    }

    public void Intercept(ResolveInvocation invocation)
    {
        if (proceed)
        {
            invocation.Proceed();
        }

        invocation.ResolvedInstance = a;
    }
}