namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class CrossWiredTransient : IWeakReferenceObservable
{
    private readonly WeakReference _reference;

    public CrossWiredTransient()
    {
        _reference = new WeakReference(this, false);
    }

    public bool HasReference => _reference.IsAlive;
}