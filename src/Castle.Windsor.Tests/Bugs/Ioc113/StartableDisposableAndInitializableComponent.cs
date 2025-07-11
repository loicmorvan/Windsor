namespace Castle.Windsor.Tests.Bugs.Ioc113;

using System;
using System.Collections.Generic;

using Castle.Core;

public class StartableDisposableAndInitializableComponent : IInitializable, IDisposable, IStartable
{
	public readonly IList<SdiComponentMethods> CalledMethods = new List<SdiComponentMethods>();

	public void Initialize()
	{
		CalledMethods.Add(SdiComponentMethods.Initialize);
	}

	public void Start()
	{
		CalledMethods.Add(SdiComponentMethods.Start);
	}

	public void DoSomething()
	{
		CalledMethods.Add(SdiComponentMethods.DoSomething);
	}

	public void Stop()
	{
		CalledMethods.Add(SdiComponentMethods.Stop);
	}

	public void Dispose()
	{
		CalledMethods.Add(SdiComponentMethods.Dispose);
	}
}