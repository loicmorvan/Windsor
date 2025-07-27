using Castle.Core.Configuration;

namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>Represents a parameter key.</summary>
public class ParameterKey
{
    internal ParameterKey(string name)
    {
        Name = name;
    }

    /// <summary>The parameter key name.</summary>
    public string Name { get; }

    /// <summary>Builds the <see cref="Parameter" /> with key/value.</summary>
    /// <param name="value">The parameter value.</param>
    /// <returns>The new <see cref="Parameter" /></returns>
    public Parameter Eq(string value)
    {
        return new Parameter(Name, value);
    }

    /// <summary>Builds the <see cref="Parameter" /> with key/config.</summary>
    /// <param name="configNode">The parameter configuration.</param>
    /// <returns>The new <see cref="Parameter" /></returns>
    public Parameter Eq(IConfiguration configNode)
    {
        return new Parameter(Name, configNode);
    }
}