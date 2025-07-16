// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Tests.Lifecycle;

using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.Facilities.TypedFactory.Components;

using CastleTests;
using CastleTests.Components;

public class DisposeTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Disposable_component_for_nondisposable_service_built_via_factory_should_be_disposed_when_released()
	{
		SimpleServiceDisposable.DisposedCount = 0;
		Container.Register(Component.For<ISimpleService>()
			.UsingFactoryMethod(() => new SimpleServiceDisposable())
			.LifeStyle.Transient);

		var service = Container.Resolve<ISimpleService>();

		Assert.Equal(0, SimpleServiceDisposable.DisposedCount);

		Container.Release(service);

		Assert.Equal(1, SimpleServiceDisposable.DisposedCount);
	}

	[Fact]
	public void Disposable_component_for_nondisposable_service_is_tracked()
	{
		Container.Register(Component.For<ISimpleService>()
			.ImplementedBy<SimpleServiceDisposable>()
			.LifeStyle.Transient);

		var service = Container.Resolve<ISimpleService>();

		Assert.True(Kernel.ReleasePolicy.HasTrack(service));
	}

	[Fact]
	public void Disposable_component_for_nondisposable_service_should_be_disposed_when_released()
	{
		SimpleServiceDisposable.DisposedCount = 0;
		Container.Register(Component.For<ISimpleService>()
			.ImplementedBy<SimpleServiceDisposable>()
			.LifeStyle.Transient);

		var service = Container.Resolve<ISimpleService>();
		Container.Release(service);

		Assert.Equal(1, SimpleServiceDisposable.DisposedCount);
	}

	[Fact]
	public void Disposable_service_is_tracked()
	{
		Container.Register(Component.For<DisposableFoo>().LifeStyle.Transient);

		var foo = Container.Resolve<DisposableFoo>();

		Assert.True(Kernel.ReleasePolicy.HasTrack(foo));
	}

	[Fact]
	public void Disposable_services_should_be_disposed_when_released()
	{
		DisposableFoo.ResetDisposedCount();
		Container.Register(Component.For<DisposableFoo>().LifeStyle.Transient);

		var foo = Container.Resolve<DisposableFoo>();
		Container.Release(foo);

		Assert.Equal(1, DisposableFoo.DisposedCount);
	}

	[Fact]
	public void Disposable_singleton_dependency_of_transient_open_generic_is_disposed()
	{
		DisposableFoo.ResetDisposedCount();
		Container.Register(
			Component.For(typeof(GenericComponent<>)).LifeStyle.Transient,
			Component.For<DisposableFoo>().LifeStyle.Singleton
		);

		var tracker = ReferenceTracker
			.Track(() =>
			{
				var depender = Container.Resolve<GenericComponent<DisposableFoo>>();
				return depender.Value;
			});

		Container.Dispose();

		Assert.Equal(1, DisposableFoo.DisposedCount);
		tracker.AssertNoLongerReferenced();
	}

	[Fact]
	public void Disposable_singleton_generic_closed_disposed()
	{
		Container.Register(Component.For<DisposableGeneric<A>>());
		var component = Container.Resolve<DisposableGeneric<A>>();

		Container.Dispose();

		Assert.True(component.Disposed);
	}

	[Fact]
	public void Disposable_singleton_generic_closed_inherited_disposed()
	{
		Container.Register(Component.For<DisposableGenericA>());
		var component = Container.Resolve<DisposableGenericA>();

		Container.Dispose();

		Assert.True(component.Disposed);
	}

	[Fact]
	public void Disposable_singleton_generic_open_disposed()
	{
		Container.Register(Component.For(typeof(DisposableGeneric<>)));
		var component = Container.Resolve<DisposableGeneric<A>>();

		Container.Dispose();

		Assert.True(component.Disposed);
	}

	[Fact]
	public void Disposable_transient_generic_closed_disposed()
	{
		Container.Register(Component.For<DisposableGeneric<A>>().LifeStyle.Transient);
		var component = Container.Resolve<DisposableGeneric<A>>();

		Container.Dispose();

		Assert.True(component.Disposed);
	}

	[Fact]
	public void Disposable_transient_generic_closed_inherited_disposed()
	{
		Container.Register(Component.For<DisposableGenericA>().LifeStyle.Transient);
		var component = Container.Resolve<DisposableGenericA>();

		Container.Dispose();

		Assert.True(component.Disposed);
	}

	[Fact]
	public void Disposable_transient_generic_open_disposed()
	{
		Container.Register(Component.For(typeof(DisposableGeneric<>)).LifeStyle.Transient);
		var component = Container.Resolve<DisposableGeneric<A>>();

		Container.Dispose();

		Assert.True(component.Disposed);
	}
}