namespace Castle.Windsor.Tests;

public interface IDevice
{
    MessageChannel Channel { get; }
    IEnumerable<IDevice> Children { get; }
}