using System;
using System.Collections.Generic;
using Castle.Windsor.Core;

namespace Castle.Windsor.Tests.Bugs.Ioc113;

public class StartableDisposableAndInitializableComponent : IInitializable, IDisposable, IStartable
{
	public readonly IList<SdiComponentMethods> CalledMethods;

	public StartableDisposableAndInitializableComponent()
	{
		CalledMethods = new List<SdiComponentMethods>();
	}

	public void Dispose()
	{
		CalledMethods.Add(SdiComponentMethods.Dispose);
	}

	public void Initialize()
	{
		CalledMethods.Add(SdiComponentMethods.Initialize);
	}

	public void Start()
	{
		CalledMethods.Add(SdiComponentMethods.Start);
	}

	public void Stop()
	{
		CalledMethods.Add(SdiComponentMethods.Stop);
	}

	public void DoSomething()
	{
		CalledMethods.Add(SdiComponentMethods.DoSomething);
	}
}