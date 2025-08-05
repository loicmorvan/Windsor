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

    public void Start(int fake)
    {
    }

    public void Stop()
    {
        StopCalled = true;
    }

    public void Stop(string fake)
    {
    }
}