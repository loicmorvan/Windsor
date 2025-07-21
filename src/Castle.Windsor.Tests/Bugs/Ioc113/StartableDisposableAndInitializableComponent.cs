namespace Castle.Windsor.Tests.Bugs.Ioc113;

using System;
using System.Collections.Generic;

using Castle.Windsor.Core;

public class StartableDisposableAndInitializableComponent : IInitializable, IDisposable, IStartable
{
	public IList<SdiComponentMethods> calledMethods;

	public StartableDisposableAndInitializableComponent()
	{
		calledMethods = new List<SdiComponentMethods>();
	}

	public void Dispose()
	{
		calledMethods.Add(SdiComponentMethods.Dispose);
	}

	public void Initialize()
	{
		calledMethods.Add(SdiComponentMethods.Initialize);
	}

	public void Start()
	{
		calledMethods.Add(SdiComponentMethods.Start);
	}

	public void Stop()
	{
		calledMethods.Add(SdiComponentMethods.Stop);
	}

	public void DoSomething()
	{
		calledMethods.Add(SdiComponentMethods.DoSomething);
	}
}