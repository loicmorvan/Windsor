namespace Castle.Windsor.MicroKernel;

/// <summary>Represents a delegate which holds a handler</summary>
/// <param name="handler">handler that holds a component and is capable of creating an instance of it.</param>
/// <param name="stateChanged"></param>
public delegate void HandlerDelegate(IHandler handler, ref bool stateChanged);