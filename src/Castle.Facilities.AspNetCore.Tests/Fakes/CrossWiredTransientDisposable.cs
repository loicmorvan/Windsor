using System;

namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class CrossWiredTransientDisposable : CrossWiredTransient, IDisposable, IDisposableObservable
{
    public void Dispose()
    {
        Disposed = true;
        DisposedCount++;
    }

    public bool Disposed { get; set; }
    public int DisposedCount { get; set; }
}