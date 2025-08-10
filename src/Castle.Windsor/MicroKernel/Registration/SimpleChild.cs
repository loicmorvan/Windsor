using Castle.Core.Configuration;

namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>Represents a simple child node.</summary>
public class SimpleChild : Node
{
    private readonly string _value;

    internal SimpleChild(string name, string value)
        : base(name)
    {
        _value = value;
    }

    /// <summary>Applies the configuration node.</summary>
    /// <param name="configuration">The configuration.</param>
    public override void ApplyTo(IConfiguration configuration)
    {
        var node = new MutableConfiguration(Name, _value);
        configuration.Children.Add(node);
    }
}