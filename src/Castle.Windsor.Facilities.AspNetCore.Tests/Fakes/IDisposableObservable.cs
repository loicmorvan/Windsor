namespace Castle.Windsor.Facilities.AspNetCore.Tests.Fakes;

public interface IDisposableObservable
{
    bool Disposed { get; set; }
    int DisposedCount { get; set; }
}