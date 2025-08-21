using Castle.Windsor.MicroKernel.Registration;

namespace Castle.Windsor.MicroKernel.ModelBuilder.Descriptors;

public class AttributeKeyDescriptor<TS>
    where TS : class
{
    private readonly ComponentRegistration<TS> _component;
    private readonly string _name;

    /// <summary>Constructs the <see cref="AttributeKeyDescriptor{S}" /> descriptor with name.</summary>
    /// <param name="component">The component.</param>
    /// <param name="name">The attribute name.</param>
    public AttributeKeyDescriptor(ComponentRegistration<TS> component, string name)
    {
        _component = component;
        _name = name;
    }

    /// <summary>Builds the <see cref="AttributeKeyDescriptor{S}" /> with value.</summary>
    /// <param name="value">The attribute value.</param>
    /// <returns>The <see cref="ComponentRegistration{S}" /></returns>
    public ComponentRegistration<TS> Eq(object? value)
    {
        var attribValue = value != null ? value.ToString() : string.Empty;
        return _component.AddAttributeDescriptor(_name, attribValue ?? string.Empty);
    }
}