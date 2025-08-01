using Castle.Core.Configuration;
using JetBrains.Annotations;

namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>Represents a named child.</summary>
public class NamedChild : Node
{
    internal NamedChild(string name)
        : base(name)
    {
    }

    /// <summary>Applies the configuration node.</summary>
    /// <param name="configuration">The configuration.</param>
    public override void ApplyTo(IConfiguration configuration)
    {
        var node = new MutableConfiguration(Name);
        configuration.Children.Add(node);
    }

    /// <summary>Builds the <see cref="SimpleChild" /> with name/value.</summary>
    /// <param name="value">The child value.</param>
    /// <returns>The new <see cref="SimpleChild" /></returns>
    public SimpleChild Eq(string value)
    {
        return new SimpleChild(Name, value);
    }

    /// <summary>Builds the <see cref="SimpleChild" /> with name/value.</summary>
    /// <param name="value">The child value.</param>
    /// <returns>The new <see cref="SimpleChild" /></returns>
    [PublicAPI]
    public SimpleChild Eq(object value)
    {
        var valueStr = value != null ? value.ToString() : string.Empty;
        return new SimpleChild(Name, valueStr);
    }

    /// <summary>Builds the <see cref="ComplexChild" /> with name/config.</summary>
    /// <param name="configNode">The child configuration.</param>
    /// <returns>The new <see cref="ComplexChild" /></returns>
    [PublicAPI]
    public ComplexChild Eq(IConfiguration configNode)
    {
        return new ComplexChild(Name, configNode);
    }

    /// <summary>Builds the <see cref="Child" /> with name/config.</summary>
    /// <param name="childNodes">The child nodes.</param>
    /// <returns>The new <see cref="CompoundChild" /></returns>
    public CompoundChild Eq(params Node[] childNodes)
    {
        return new CompoundChild(Name, childNodes);
    }
}