using Castle.Windsor.MicroKernel.SubSystems.Conversion;
using JetBrains.Annotations;

namespace Castle.Windsor.Tests.Config.Components;

/// <summary>The server.</summary>
[Convertible]
[UsedImplicitly]
public class Server
{
    /// <summary>Gets or sets the ip.</summary>
    public string Ip { get; set; }

    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; }
}