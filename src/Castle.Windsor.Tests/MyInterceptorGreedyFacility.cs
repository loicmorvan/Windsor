using Castle.Core.Configuration;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel;

namespace Castle.Windsor.Tests;

public class MyInterceptorGreedyFacility : IFacility
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
        if (key == "key")
        {
            handler.ComponentModel.Interceptors.Add(
                new InterceptorReference("interceptor"));
        }
    }
}