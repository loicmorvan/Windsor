namespace Castle.Windsor.MicroKernel;

/// <summary>Represents a delegate which holds basic information about a component.</summary>
/// <param name="key">Key which identifies the component</param>
/// <param name="handler">handler that holds this component and is capable of creating an instance of it.</param>
public delegate void ComponentDataDelegate(string key, IHandler handler);