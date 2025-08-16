using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

[UsedImplicitly]
public class TestDevice : BaseDevice
{
    private readonly List<IDevice> _children;

    public TestDevice()
    {
    }

    public TestDevice(IEnumerable<IDevice> theChildren)
    {
        _children = new List<IDevice>(theChildren);
    }

    public virtual IEnumerable<IDevice> Children => _children;
}