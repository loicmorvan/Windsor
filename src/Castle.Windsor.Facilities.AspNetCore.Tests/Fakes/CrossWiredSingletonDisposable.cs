namespace Castle.Windsor.Facilities.AspNetCore.Tests.Fakes;

public sealed class CrossWiredSingletonDisposable : CrossWiredSingleton, IDisposable, IDisposableObservable
{
    public void Dispose()
    {
        Disposed = true;
        DisposedCount++;
    }

    public bool Disposed { get; private set; }
    public int DisposedCount { get; private set; }
}