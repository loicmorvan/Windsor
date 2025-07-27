using System;

namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class ServiceProviderOnlyScoped : IWeakReferenceObservable
{
    private readonly WeakReference _reference;

    public ServiceProviderOnlyScoped()
    {
        _reference = new WeakReference(this, false);
    }

    public bool HasReference => _reference.IsAlive;
}