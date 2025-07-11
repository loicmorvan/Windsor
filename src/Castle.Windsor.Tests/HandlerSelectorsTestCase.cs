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

using System;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;

namespace Castle.Windsor.Tests;

public class HandlerSelectorsTestCase
{
	public enum Interest
	{
		None,
		Biology,
		Astronomy
	}

	[Fact]
	public void SelectUsingBusinessLogic_DirectSelection()
	{
		IWindsorContainer container = new WindsorContainer();
		container.Register(Component.For<IWatcher>().ImplementedBy<BirdWatcher>().Named("bird.watcher")).Register(
			Component.For<IWatcher>().ImplementedBy<SatiWatcher>().Named("astronomy.watcher"));
		var selector = new WatcherSelector();
		container.Kernel.AddHandlerSelector(selector);

		Assert.IsType<BirdWatcher>(container.Resolve<IWatcher>("default"));
		selector.Interest = Interest.Astronomy;
		Assert.IsType<SatiWatcher>(container.Resolve<IWatcher>("change-by-context"));
		selector.Interest = Interest.Biology;
		Assert.IsType<BirdWatcher>(container.Resolve<IWatcher>("explicit"));
	}

	[Fact]
	public void SelectUsingBusinessLogic_SubDependency()
	{
		IWindsorContainer container = new WindsorContainer();
		container.Register(Component.For(typeof(Person)).LifeStyle.Is(LifestyleType.Transient)).Register(
			Component.For<IWatcher>().ImplementedBy<BirdWatcher>().Named("bird.watcher")).Register(
			Component.For<IWatcher>().ImplementedBy<SatiWatcher>().Named("astronomy.watcher"));
		var selector = new WatcherSelector();
		container.Kernel.AddHandlerSelector(selector);

		Assert.IsType<BirdWatcher>(container.Resolve<Person>("default").Watcher);
		selector.Interest = Interest.Astronomy;
		Assert.IsType<SatiWatcher>(container.Resolve<Person>("change-by-context").Watcher);
		selector.Interest = Interest.Biology;
		Assert.IsType<BirdWatcher>(container.Resolve<Person>("explicit").Watcher);
	}

	[Fact]
	public void SubDependencyResolverHasHigherPriorityThanHandlerSelector()
	{
		IWindsorContainer container = new WindsorContainer();
		container.Register(Component.For(typeof(Person)).LifeStyle.Is(LifestyleType.Transient)).Register(
			Component.For<IWatcher>().ImplementedBy<BirdWatcher>().Named("bird.watcher")).Register(
			Component.For<IWatcher>().ImplementedBy<SatiWatcher>().Named("astronomy.watcher"));
		var selector = new WatcherSelector();
		container.Kernel.AddHandlerSelector(selector);
		container.Kernel.Resolver.AddSubResolver(new WatchSubDependencySelector());

		selector.Interest = Interest.Biology;
		Assert.IsType<SatiWatcher>(container.Resolve<Person>("sub dependency should resolve sati").Watcher);
		Assert.IsType<BirdWatcher>(container.Resolve<IWatcher>("root dependency should resolve bird"));
	}

	public class BirdWatcher : IWatcher
	{
		public event Action<string> OnSomethingInterestingToWatch = delegate { };
	}

	public interface IWatcher
	{
		event Action<string> OnSomethingInterestingToWatch;
	}

	public class PeopleWatcher(Person p)
	{
		private Person _p = p;
	}

	public class Person(IWatcher watcher)
	{
		public readonly IWatcher Watcher = watcher;
	}

	public class SatiWatcher : IWatcher
	{
		public event Action<string> OnSomethingInterestingToWatch = delegate { };
	}

	public class WatchSubDependencySelector : ISubDependencyResolver
	{
		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
			ComponentModel model,
			DependencyModel dependency)
		{
			return dependency.TargetType == typeof(IWatcher);
		}

		public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
			ComponentModel model,
			DependencyModel dependency)
		{
			return new SatiWatcher();
		}
	}

	public class WatcherSelector : IHandlerSelector
	{
		public Interest Interest = Interest.None;

		public bool HasOpinionAbout(string key, Type service)
		{
			return Interest != Interest.None && service == typeof(IWatcher);
		}

		public IHandler SelectHandler(string key, Type service, IHandler[] handlers)
		{
			foreach (var handler in handlers)
				if (handler.ComponentModel.Name.ToUpper().Contains(Interest.ToString().ToUpper()))
					return handler;

			return null;
		}
	}
}