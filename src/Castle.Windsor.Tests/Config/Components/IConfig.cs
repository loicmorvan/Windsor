namespace Castle.Windsor.Tests.Config.Components;

/// <summary>The Config interface.</summary>
public interface IConfig
{
    /// <summary>Gets or sets the name.</summary>
    string Name { get; set; }

    /// <summary>Gets or sets the servers.</summary>
    Server[] Servers { get; set; }
}