using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public sealed class AnyMiddleware : IMiddleware
{
    [UsedImplicitly]
    public AnyMiddleware(
        ServiceProviderOnlyScopedDisposable serviceProviderOnlyScopedDisposable,
        WindsorOnlyScopedDisposable windsorOnlyScopedDisposable,
        CrossWiredScopedDisposable crossWiredScopedDisposable)
    {
        ArgumentNullException.ThrowIfNull(serviceProviderOnlyScopedDisposable);
        ArgumentNullException.ThrowIfNull(windsorOnlyScopedDisposable);
        ArgumentNullException.ThrowIfNull(crossWiredScopedDisposable);
    }

    [UsedImplicitly]
    public AnyMiddleware(AnyComponent anyComponent)
    {
        // This will never get called because Windsor picks the most greedy constructor
        ArgumentNullException.ThrowIfNull(anyComponent);
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Do something before
        await next(context);
        // Do something after
    }
}