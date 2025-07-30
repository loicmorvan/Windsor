namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class WindsorOnlyScoped : IWeakReferenceObservable
{
    private readonly WeakReference _reference;

    public WindsorOnlyScoped()
    {
        _reference = new WeakReference(this, false);
    }

    public bool HasReference => _reference.IsAlive;
}