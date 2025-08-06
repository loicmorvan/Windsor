using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

[UsedImplicitly]
public class UsingString(string parameter)
{
    public string Parameter { get; } = parameter;
}