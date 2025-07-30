using Castle.Core.Configuration;
using Castle.DynamicProxy;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel;

namespace Castle.Windsor.Tests;

public class MyInterceptorGreedyFacility2 : IFacility
{
    public void Init(IKernel kernel, IConfiguration facilityConfig)
    {
        kernel.ComponentRegistered += OnComponentRegistered;
    }

    public void Terminate()
    {
    }

    private void OnComponentRegistered(string key, IHandler handler)
    {
        if (handler.ComponentModel.Services.Any(s => s.Is<IInterceptor>()))
        {
            return;
        }

        handler.ComponentModel.Interceptors.Add(new InterceptorReference("interceptor"));
    }
}