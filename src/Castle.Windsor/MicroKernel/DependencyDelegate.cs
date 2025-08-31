using Castle.Windsor.Core;

namespace Castle.Windsor.MicroKernel;

/// <summary>Represents a delegate which holds dependency resolving information.</summary>
public delegate void DependencyDelegate(ComponentModel client, DependencyModel model, object? dependency);