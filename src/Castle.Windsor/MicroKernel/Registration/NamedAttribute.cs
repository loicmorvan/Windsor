using JetBrains.Annotations;

namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>Represents a named attribute.</summary>
public class NamedAttribute
{
    private readonly string _name;

    internal NamedAttribute(string name)
    {
        _name = name;
    }

    /// <summary>Builds the <see cref="Attribute" /> with name/value.</summary>
    /// <param name="value">The attribute value.</param>
    /// <returns>The new <see cref="SimpleChild" /></returns>
    [PublicAPI]
    public Attrib Eq(string value)
    {
        return new Attrib(_name, value);
    }

    /// <summary>Builds the <see cref="Attribute" /> with name/value.</summary>
    /// <param name="value">The attribute value.</param>
    /// <returns>The new <see cref="SimpleChild" /></returns>
    public Attrib Eq(object value)
    {
        var valueStr = value != null ? value.ToString() : string.Empty;
        return new Attrib(_name, valueStr);
    }
}