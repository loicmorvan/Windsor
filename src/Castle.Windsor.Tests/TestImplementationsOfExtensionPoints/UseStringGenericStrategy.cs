namespace Castle.Windsor.Tests.TestImplementationsOfExtensionPoints;

using System;

using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Context;
using Castle.Windsor.MicroKernel.Handlers;

public class UseStringGenericStrategy : IGenericImplementationMatchingStrategy
{
	public Type[] GetGenericArguments(ComponentModel model, CreationContext context)
	{
		return [typeof(string)];
	}
}