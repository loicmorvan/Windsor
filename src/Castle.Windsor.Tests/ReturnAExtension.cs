using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Handlers;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

public class ReturnAExtension : IResolveExtension
{
    private readonly A _a;
    private readonly bool _proceed;

    public ReturnAExtension(A a, bool proceed = false)
    {
        _a = a;
        _proceed = proceed;
    }

    public void Init(IKernel kernel, IHandler handler)
    {
    }

    public void Intercept(ResolveInvocation invocation)
    {
        if (_proceed)
        {
            invocation.Proceed();
        }

        invocation.ResolvedInstance = _a;
    }
}