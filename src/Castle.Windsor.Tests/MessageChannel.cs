using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

[UsedImplicitly]
public class MessageChannel(IDevice root)
{
    public IDevice RootDevice { get; } = root;
}