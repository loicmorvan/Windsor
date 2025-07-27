using System;

namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class WindsorOnlySingletonDisposable : WindsorOnlySingleton, IDisposable, IDisposableObservable
{
    public void Dispose()
    {
        Disposed = true;
        DisposedCount++;
    }

    public bool Disposed { get; set; }
    public int DisposedCount { get; set; }
}