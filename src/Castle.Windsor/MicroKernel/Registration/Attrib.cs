using Castle.Core.Configuration;

namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>Represents a configuration attribute.</summary>
public class Attrib : Node
{
    private readonly string _value;

    internal Attrib(string name, string value)
        : base(name)
    {
        _value = value;
    }

    /// <summary>Applies the configuration node.</summary>
    /// <param name="configuration">The configuration.</param>
    public override void ApplyTo(IConfiguration configuration)
    {
        configuration.Attributes.Add(Name, _value);
    }

    /// <summary>Create a <see cref="NamedAttribute" /> with name.</summary>
    /// <param name="name">The attribute name.</param>
    /// <returns>The new <see cref="NamedAttribute" /></returns>
    public static NamedAttribute ForName(string name)
    {
        return new NamedAttribute(name);
    }
}