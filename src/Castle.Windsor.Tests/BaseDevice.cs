namespace Castle.Windsor.Tests;

public abstract class BaseDevice : IDevice
{
    public abstract IEnumerable<IDevice> Children { get; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public MessageChannel Channel { get; set; }
}