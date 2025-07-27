using System;

namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class CrossWiredSingleton : IWeakReferenceObservable
{
    private readonly WeakReference _reference;

    public CrossWiredSingleton()
    {
        _reference = new WeakReference(this, false);
    }

    public bool HasReference => _reference.IsAlive;
}