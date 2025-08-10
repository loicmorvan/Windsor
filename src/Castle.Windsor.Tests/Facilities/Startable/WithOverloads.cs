using Castle.Windsor.Core;

namespace Castle.Windsor.Tests.Facilities.Startable;

[Transient]
public class WithOverloads
{
    public bool StartCalled { get; private set; }
    public bool StopCalled { get; private set; }

    public void Start()
    {
        StartCalled = true;
    }

    // ReSharper disable once UnusedParameter.Global
    public void Start(int fake)
    {
    }

    public void Stop()
    {
        StopCalled = true;
    }

    // ReSharper disable once UnusedParameter.Global
    public void Stop(string fake)
    {
    }
}