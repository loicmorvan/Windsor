namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>Represents a configuration child.</summary>
public abstract class Child
{
    /// <summary>Create a <see cref="NamedChild" /> with name.</summary>
    /// <param name="name">The child name.</param>
    /// <returns>The new <see cref="NamedChild" /></returns>
    public static NamedChild ForName(string name)
    {
        return new NamedChild(name);
    }
}