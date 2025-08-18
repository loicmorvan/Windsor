namespace Castle.Windsor.Tests;

[AttributeUsage(AttributeTargets.Interface)]
public class DefaultImplementationAttribute(Type implementation) : Attribute
{
    public Type Implementation { get; } = implementation;
}