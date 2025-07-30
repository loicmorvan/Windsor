using Castle.Windsor.Core;
using JetBrains.Annotations;

namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>Represents a property key.</summary>
public class PropertyKey
{
    internal PropertyKey(object key)
    {
        Key = key;
    }

    /// <summary>The property key key.</summary>
    [PublicAPI]
    public object Key { get; }

    /// <summary>Builds the <see cref="Property" /> with key/value.</summary>
    /// <param name="value">The property value.</param>
    /// <returns>The new <see cref="Property" /></returns>
    public Property Eq(object value)
    {
        return new Property(Key, value);
    }

    /// <summary>
    ///     Builds a service override using other component registered with given <paramref name="componentName" /> as value
    ///     for dependency with given
    ///     <see
    ///         cref="Key" />
    ///     .
    /// </summary>
    /// <param name="componentName"></param>
    /// <returns></returns>
    public ServiceOverride Is(string componentName)
    {
        return GetServiceOverrideKey().Eq(componentName);
    }

    /// <summary>
    ///     Builds a service override using other component registered with given <paramref name="componentImplementation" />
    ///     and no explicit name, as value for dependency with given
    ///     <see
    ///         cref="Key" />
    ///     .
    /// </summary>
    /// <returns></returns>
    public ServiceOverride Is(Type componentImplementation)
    {
        ArgumentNullException.ThrowIfNull(componentImplementation);
        return GetServiceOverrideKey().Eq(ComponentName.DefaultNameFor(componentImplementation));
    }

    /// <summary>
    ///     Builds a service override using other component registered with given
    ///     <typeparam name="TComponentImplementation" />
    ///     and no explicit name, as value for dependency with given
    ///     <see
    ///         cref="Key" />
    ///     .
    /// </summary>
    /// <returns></returns>
    public ServiceOverride Is<TComponentImplementation>()
    {
        return Is(typeof(TComponentImplementation));
    }

    private ServiceOverrideKey GetServiceOverrideKey()
    {
        if (Key is Type key)
        {
            return ServiceOverride.ForKey(key);
        }

        return ServiceOverride.ForKey((string)Key);
    }
}