namespace Castle.Windsor.Facilities.AspNetCore.Tests.Fakes;

public class WindsorOnlySingleton : IWeakReferenceObservable
{
    private readonly WeakReference _reference;

    public WindsorOnlySingleton()
    {
        _reference = new WeakReference(this, false);
    }

    public bool HasReference => _reference.IsAlive;
}