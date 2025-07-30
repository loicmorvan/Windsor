namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>Represents a service override key.</summary>
public class ServiceOverrideKey
{
    private readonly object _key;

    internal ServiceOverrideKey(string key)
    {
        _key = key;
    }

    internal ServiceOverrideKey(Type key)
    {
        _key = key;
    }

    /// <summary>Builds the <see cref="ServiceOverride" /> with key/value.</summary>
    /// <param name="value">The service override value.</param>
    /// <returns>The new <see cref="ServiceOverride" /></returns>
    public ServiceOverride Eq(string value)
    {
        return new ServiceOverride(_key, value);
    }

    /// <summary>Builds the <see cref="ServiceOverride" /> with key/values.</summary>
    /// <param name="value">The service override values.</param>
    /// <returns>The new <see cref="ServiceOverride" /></returns>
    public ServiceOverride Eq(params string[] value)
    {
        return new ServiceOverride(_key, value);
    }

    /// <summary>Builds the <see cref="ServiceOverride" /> with key/values.</summary>
    /// <param name="value">The service override values.</param>
    /// <returns>The new <see cref="ServiceOverride" /></returns>
    /// <typeparam name="TV">The value type.</typeparam>
    public ServiceOverride Eq<TV>(params string[] value)
    {
        return new ServiceOverride(_key, value, typeof(TV));
    }

    /// <summary>Builds the <see cref="ServiceOverride" /> with key/values.</summary>
    /// <param name="value">The service override values.</param>
    /// <returns>The new <see cref="ServiceOverride" /></returns>
    public ServiceOverride Eq(IEnumerable<string> value)
    {
        return new ServiceOverride(_key, value);
    }

    /// <summary>Builds the <see cref="ServiceOverride" /> with key/values.</summary>
    /// <param name="value">The service override values.</param>
    /// <returns>The new <see cref="ServiceOverride" /></returns>
    /// <typeparam name="TV">The value type.</typeparam>
    public ServiceOverride Eq<TV>(IEnumerable<string> value)
    {
        return new ServiceOverride(_key, value, typeof(TV));
    }

    public ServiceOverride Eq(params Type[] componentTypes)
    {
        return new ServiceOverride(_key, componentTypes);
    }

    public ServiceOverride Eq<TV>(params Type[] componentTypes)
    {
        return new ServiceOverride(_key, componentTypes, typeof(TV));
    }
}