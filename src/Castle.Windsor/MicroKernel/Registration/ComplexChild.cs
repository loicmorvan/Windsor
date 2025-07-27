using Castle.Core.Configuration;

namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>Represents a complex child node.</summary>
public class ComplexChild : Node
{
    private readonly IConfiguration _configNode;

    internal ComplexChild(string name, IConfiguration configNode)
        : base(name)
    {
        _configNode = configNode;
    }

    /// <summary>Applies the configuration node.</summary>
    /// <param name="configuration">The configuration.</param>
    public override void ApplyTo(IConfiguration configuration)
    {
        var node = new MutableConfiguration(Name);
        node.Children.Add(_configNode);
        configuration.Children.Add(node);
    }
}