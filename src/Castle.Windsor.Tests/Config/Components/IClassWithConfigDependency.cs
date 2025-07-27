namespace Castle.Windsor.Tests.Config.Components;

/// <summary>The ClassWithConfigDependency interface.</summary>
public interface IClassWithConfigDependency
{
    /// <summary>Gets the name of the current configuration.</summary>
    /// <returns>Returns the configuration name.</returns>
    string GetName();

    /// <summary>Gets the IP of a given server.</summary>
    /// <param name="name">The name.</param>
    /// <returns>Returns the IP address of a server.</returns>
    string GetServerIp(string name);
}