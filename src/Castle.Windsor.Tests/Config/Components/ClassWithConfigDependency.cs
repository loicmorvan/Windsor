namespace Castle.Windsor.Tests.Config.Components;

using System.Linq;

/// <summary>The class with config dependency.</summary>
public class ClassWithConfigDependency : IClassWithConfigDependency
{
	/// <summary>The _config.</summary>
	private readonly IConfig _config;

	/// <summary>Initializes a new instance of the <see cref = "ClassWithConfigDependency" /> class.</summary>
	/// <param name = "config">The config.</param>
	public ClassWithConfigDependency(IConfig config)
	{
		_config = config;
	}

	/// <summary>The get name.</summary>
	/// <returns>The <see cref = "string" />.</returns>
	public string GetName()
	{
		return _config.Name;
	}

	/// <summary>The get server ip.</summary>
	/// <param name = "name">The name.</param>
	/// <returns>The <see cref = "string" />.</returns>
	public string GetServerIp(string name)
	{
		return _config.Servers.First(s => s.Name == name).Ip;
	}
}