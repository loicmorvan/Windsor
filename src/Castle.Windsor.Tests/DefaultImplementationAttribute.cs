namespace Castle.Windsor.Tests;

[AttributeUsage(AttributeTargets.Interface)]
public class DefaultImplementationAttribute : Attribute
{
    public DefaultImplementationAttribute(Type implementation)
    {
        Implementation = implementation;
    }

    public Type Implementation { get; }
}