using Castle.Core.Configuration;

namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>Represents a compound child node.</summary>
public class CompoundChild : Node
{
    private readonly Node[] _childNodes;

    internal CompoundChild(string name, Node[] childNodes)
        : base(name)
    {
        _childNodes = childNodes;
    }

    /// <summary>Applies the configuration node.</summary>
    /// <param name="configuration">The configuration.</param>
    public override void ApplyTo(IConfiguration configuration)
    {
        var node = new MutableConfiguration(Name);
        foreach (var childNode in _childNodes)
        {
            childNode.ApplyTo(node);
        }

        configuration.Children.Add(node);
    }
}