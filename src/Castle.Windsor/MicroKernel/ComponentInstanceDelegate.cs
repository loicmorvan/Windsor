using Castle.Windsor.Core;

namespace Castle.Windsor.MicroKernel;

/// <summary>Represents a delegate which holds basic information about a component and its instance.</summary>
/// <param name="model">Component meta information</param>
/// <param name="instance">Component instance</param>
public delegate void ComponentInstanceDelegate(ComponentModel model, object instance);