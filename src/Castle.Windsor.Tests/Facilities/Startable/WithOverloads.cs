using Castle.Windsor.Core;

namespace Castle.Windsor.Tests.Facilities.Startable;

[Transient]
public class WithOverloads
{
    public bool StartCalled { get; private set; }
    public bool StopCalled { get; private set; }

    // ReSharper disable once UnusedMember.Global
    public void Start()
    {
        StartCalled = true;
    }

    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMember.Global
#pragma warning disable CA1822
    public void Start(int fake)
#pragma warning restore CA1822
    {
    }

    // ReSharper disable once UnusedMember.Global
    public void Stop()
    {
        StopCalled = true;
    }

    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMember.Global
#pragma warning disable CA1822
    public void Stop(string fake)
#pragma warning restore CA1822
    {
    }
}