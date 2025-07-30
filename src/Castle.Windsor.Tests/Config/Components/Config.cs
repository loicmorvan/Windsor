namespace Castle.Windsor.Tests.Config.Components;

/// <summary>The config.</summary>
public class Config : IConfig
{
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets the servers.</summary>
    public Server[] Servers { get; set; }
}