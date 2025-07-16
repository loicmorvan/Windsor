// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Windsor.Tests;

using System;

using Castle.MicroKernel.Registration;

public class ConfigureDecoratorsTestCase
{
	private interface IDoNothingService
	{
		void DoNothing();
	}

	private interface IDoSomethingService
	{
		void DoSomething();
	}

	private class DoNothingService : IDoNothingService
	{
		public void DoNothing()
		{
			throw new NotImplementedException();
		}
	}

	private class DoNothingServiceDecorator : IDoNothingService
	{
		public DoNothingServiceDecorator(IDoNothingService inner)
		{
			Inner = inner;
		}

		public IDoNothingService Inner { get; }

		public void DoNothing()
		{
			throw new NotImplementedException();
		}
	}

	private class DoSomethingService : IDoSomethingService
	{
		public DoSomethingService(IDoNothingService service)
		{
		}

		public void DoSomething()
		{
			throw new NotImplementedException();
		}
	}

	[Fact]
	public void ShouldResolveComponentFromParent()
	{
		var parent = new WindsorContainer();
		var child = new WindsorContainer();
		parent.AddChildContainer(child);
		parent.Register(
			Component.For<IDoNothingService>().ImplementedBy<DoNothingService>().Named("DoNothingService"),
			Component.For<IDoSomethingService>().ImplementedBy<DoSomethingService>().Named("DoSomethingService"));

		Assert.NotNull(child.Resolve<IDoNothingService>());
		Assert.NotNull(child.Resolve<IDoSomethingService>());
	}

	[Fact]
	public void ShouldResolveDecoratedComponent()
	{
		var container = new WindsorContainer();
		container.Register(
			Component.For<IDoNothingService>().ImplementedBy<DoNothingServiceDecorator>().Named("DoNothingServiceDecorator"),
			Component.For<IDoNothingService>().ImplementedBy<DoNothingService>().Named("DoNothingService"));
		var service = container.Resolve<IDoNothingService>();

		Assert.IsType<DoNothingServiceDecorator>(service);
		Assert.IsType<DoNothingService>(((DoNothingServiceDecorator)service).Inner);
	}

	[Fact]
	public void ShouldResolveDecoratedComponentFromGrandParent()
	{
		var grandParent = new WindsorContainer();
		var parent = new WindsorContainer();
		var child = new WindsorContainer();
		grandParent.AddChildContainer(parent);
		parent.AddChildContainer(child);
		grandParent.Register(
			Component.For(typeof(IDoNothingService)).ImplementedBy(typeof(DoNothingServiceDecorator)).Named(
				"DoNothingServiceDecorator"));
		grandParent.Register(
			Component.For(typeof(IDoNothingService)).ImplementedBy(typeof(DoNothingService)).Named("DoNothingService"));
		var service = child.Resolve<IDoNothingService>();
		Assert.NotNull(service);
		Assert.IsType<DoNothingServiceDecorator>(service);
	}

	[Fact]
	public void ShouldResolveDecoratedComponentFromParent()
	{
		var parent = new WindsorContainer();
		var child = new WindsorContainer();
		parent.AddChildContainer(child);
		parent.Register(
			Component.For(typeof(IDoNothingService)).ImplementedBy(typeof(DoNothingServiceDecorator)).Named(
				"DoNothingServiceDecorator"));
		parent.Register(
			Component.For(typeof(IDoNothingService)).ImplementedBy(typeof(DoNothingService)).Named("DoNothingService"));
		child.Register(
			Component.For(typeof(IDoSomethingService)).ImplementedBy(typeof(DoSomethingService)).Named("DoSometingService"));
		var service = child.Resolve<IDoNothingService>();
		Assert.NotNull(service);
		Assert.IsType<DoNothingServiceDecorator>(service);
	}
}