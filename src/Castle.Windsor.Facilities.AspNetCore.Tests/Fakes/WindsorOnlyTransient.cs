namespace Castle.Windsor.Facilities.AspNetCore.Tests.Fakes;

public class WindsorOnlyTransient : IWeakReferenceObservable
{
    private readonly WeakReference _reference;

    public WindsorOnlyTransient()
    {
        _reference = new WeakReference(this, false);
    }

    public bool HasReference => _reference.IsAlive;
}