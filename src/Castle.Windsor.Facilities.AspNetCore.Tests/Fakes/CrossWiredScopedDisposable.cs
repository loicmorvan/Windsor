namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public sealed class CrossWiredScopedDisposable : CrossWiredScoped, IDisposable, IDisposableObservable
{
    public void Dispose()
    {
        Disposed = true;
        DisposedCount++;
    }

    public bool Disposed { get; set; }
    public int DisposedCount { get; set; }
}