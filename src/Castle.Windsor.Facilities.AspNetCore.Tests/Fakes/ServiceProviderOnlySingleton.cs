namespace Castle.Windsor.Facilities.AspNetCore.Tests.Fakes;

public class ServiceProviderOnlySingleton : IWeakReferenceObservable
{
    private readonly WeakReference _reference;

    public ServiceProviderOnlySingleton()
    {
        _reference = new WeakReference(this, false);
    }

    public bool HasReference => _reference.IsAlive;
}