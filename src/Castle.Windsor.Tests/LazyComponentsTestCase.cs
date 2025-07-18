// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace CastleTests;

using System;
using System.Reflection;
using System.Threading;

using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor.Tests.Lifecycle;

using CastleTests.Components;
using CastleTests.Facilities.TypedFactory;

public class LazyComponentsTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Can_provide_lazy_as_dependency()
	{
		Container.Register(Component.For(typeof(UsesLazy<>)).LifeStyle.Transient,
			Component.For<A>());

		var value = Container.Resolve<UsesLazy<A>>();

		Assert.NotNull(value.Lazy);
		Assert.NotNull(value.Lazy.Value);
	}

	[Fact]
	public void Can_resolve_component_via_lazy()
	{
		Container.Register(Component.For<A>());

		var lazy = Container.Resolve<Lazy<A>>();
		var a = lazy.Value;

		Assert.NotNull(a);
	}

	[Fact]
	public void Can_resolve_lazy_before_actual_component_is_registered()
	{
		var lazy = Container.Resolve<Lazy<A>>();

		Container.Register(Component.For<A>());

		Assert.NotNull(lazy.Value);
	}

	[Fact]
	public void Can_resolve_lazy_before_dependencies_of_actual_component_are_registered()
	{
		Container.Register(Component.For<B>());

		var lazy = Container.Resolve<Lazy<B>>();

		Container.Register(Component.For<A>());

		var b = lazy.Value;
		Assert.NotNull(b);
		Assert.NotNull(b.A);
	}

	[Fact]
	public void Can_resolve_lazy_component()
	{
		Container.Register(Component.For<A>());

		Container.Resolve<Lazy<A>>();
	}

	[Fact]
	public void Implicit_lazy_can_handle_generic_component()
	{
		Container.Register(Component.For(typeof(EmptyGenericClassService<>)));

		var lazy1 = Container.Resolve<Lazy<EmptyGenericClassService<A>>>();
		var lazy2 = Container.Resolve<Lazy<EmptyGenericClassService<B>>>();

		Assert.NotNull(lazy1.Value);
		Assert.NotNull(lazy2.Value);
	}

	[Fact]
	public void Implicit_lazy_is_always_tracked_by_release_policy()
	{
		Container.Register(Component.For<A>());

		var lazy = Container.Resolve<Lazy<A>>();

		Assert.True(Kernel.ReleasePolicy.HasTrack(lazy));
	}

	[Fact]
	public void Implicit_lazy_is_initialized_once()
	{
		Container.Register(Component.For<A>());

		var lazy = Container.Resolve<Lazy<A>>();
		var mode = GetMode(lazy);

		Assert.Equal(LazyThreadSafetyMode.ExecutionAndPublication, mode);
	}

	[Fact]
	public void Implicit_lazy_is_transient()
	{
		Container.Register(Component.For<A>());

		var lazy1 = Container.Resolve<Lazy<A>>();
		var lazy2 = Container.Resolve<Lazy<A>>();

		Assert.NotSame(lazy1, lazy2);

		var handler = Kernel.GetHandler(typeof(Lazy<A>));
		Assert.Equal(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
	}

	[Fact]
	public void Can_resolve_same_component_via_two_lazy()
	{
		Container.Register(Component.For<A>(),
			Component.For<B>());

		var lazy1 = Container.Resolve<Lazy<A>>();
		var lazy2 = Container.Resolve<Lazy<B>>();

		Assert.NotNull(lazy1.Value);
		Assert.NotNull(lazy2.Value);
	}

	[Fact]
	public void Can_resolve_lazy_component_requiring_arguments_inline()
	{
		Container.Register(Component.For<B>());

		var a = new A();
		var arguments = new Arguments().AddTyped(a);
		var missingArguments = Container.Resolve<Lazy<B>>();
		var hasArguments = Container.Resolve<Lazy<B>>(Arguments.FromProperties(new { arguments }));

		B ignore;
		Assert.Throws<DependencyResolverException>(() => ignore = missingArguments.Value);

		Assert.NotNull(hasArguments.Value);
		Assert.Same(a, hasArguments.Value.A);
	}

	[Fact]
	public void Can_resolve_lazy_component_with_override()
	{
		Container.Register(Component.For<A>().Named("1"),
			Component.For<A>().Named("2"));

		var lazyA = Container.Resolve<Lazy<A>>(Arguments.FromProperties(new { overrideComponentName = "2" }));

		var a2 = Container.Resolve<A>("2");
		Assert.Same(a2, lazyA.Value);
	}

	[Fact]
	public void Can_resolve_various_components_via_lazy()
	{
		Container.Register(Component.For<A>());

		var lazy1 = Container.Resolve<Lazy<A>>();
		var lazy2 = Container.Resolve<Lazy<A>>();

		Assert.NotSame(lazy1, lazy2);
		Assert.Same(lazy1.Value, lazy2.Value);
	}

	[Fact]
	public void Implicit_lazy_resolves_default_component_for_given_service_take_1()
	{
		Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>());

		var lazy = Container.Resolve<Lazy<IEmptyService>>();

		Assert.IsType<EmptyServiceA>(lazy.Value);
	}

	[Fact]
	public void Implicit_lazy_resolves_default_component_for_given_service_take_2()
	{
		Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>());

		var lazy = Container.Resolve<Lazy<IEmptyService>>();

		Assert.IsType<EmptyServiceB>(lazy.Value);
	}

	[Fact]
	public void Lazy_throws_on_resolve_when_no_component_present_for_requested_service()
	{
		var lazy = Container.Resolve<Lazy<A>>();

		Assert.Throws<ComponentNotFoundException>(() =>
		{
			var ignoreMe = lazy.Value;
		});
	}

	[Fact]
	public void Lazy_of_string_is_not_resolvable()
	{
		Assert.Throws<ComponentNotFoundException>(() => Container.Resolve<Lazy<string>>());
	}

	[Fact]
	public void Releasing_lazy_releases_requested_component()
	{
		var counter = new LifecycleCounter();

		Container.Register(
			Component.For<LifecycleCounter>().Instance(counter),
			Component.For<DisposeTestCase.Disposable>().LifeStyle.Transient);

		var lazy = Container.Resolve<Lazy<DisposeTestCase.Disposable>>();

		Assert.Equal(0, counter.InstancesDisposed);
		var value = lazy.Value;

		Container.Release(lazy);
		Assert.Equal(1, counter.InstancesDisposed);
	}

	[Fact]
	public void Resolving_lazy_doesnt_resolve_requested_component_eagerly()
	{
		HasInstanceCount.ResetInstancesCreated();

		Container.Register(Component.For<HasInstanceCount>());

		var lazy = Container.Resolve<Lazy<HasInstanceCount>>();

		Assert.Equal(0, HasInstanceCount.InstancesCreated);
		Assert.False(lazy.IsValueCreated);

		var value = lazy.Value;

		Assert.Equal(1, HasInstanceCount.InstancesCreated);
	}

	protected override void AfterContainerCreated()
	{
		Container.Register(Component.For<ILazyComponentLoader>()
			.ImplementedBy<LazyOfTComponentLoader>());
	}

	private LazyThreadSafetyMode GetMode(Lazy<A> lazy)
	{
		return (LazyThreadSafetyMode)lazy.GetType().GetProperty("Mode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(lazy, null);
	}
}