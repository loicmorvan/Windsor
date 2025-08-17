using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

[UsedImplicitly]
public class TestDevice : BaseDevice
{
    public TestDevice()
    {
    }

    public TestDevice(IEnumerable<IDevice> theChildren)
    {
    }
}