namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class CrossWiredScoped : IWeakReferenceObservable
{
    private readonly WeakReference _reference;

    public CrossWiredScoped()
    {
        _reference = new WeakReference(this, false);
    }

    public bool HasReference => _reference.IsAlive;
}