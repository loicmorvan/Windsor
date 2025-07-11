using System;
using Castle.Core;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Handlers;

namespace Castle.Windsor.Tests.TestImplementationsOfExtensionPoints;

public class UseStringGenericStrategy : IGenericImplementationMatchingStrategy
{
	public Type[] GetGenericArguments(ComponentModel model, CreationContext context)
	{
		return [typeof(string)];
	}
}