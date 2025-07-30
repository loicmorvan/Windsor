namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class ServiceProviderOnlyTransient : IWeakReferenceObservable
{
    private readonly WeakReference _reference;

    public ServiceProviderOnlyTransient()
    {
        _reference = new WeakReference(this, false);
    }

    public bool HasReference => _reference.IsAlive;
}