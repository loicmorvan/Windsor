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

using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Releasers;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Windsor;

namespace Castle.Windsor.Tests;

public class LifecycledComponentsReleasePolicyTestCase
{
	private readonly IWindsorContainer _container;
	private readonly IReleasePolicy _releasePolicy;

	public LifecycledComponentsReleasePolicyTestCase()
	{
		_container = new WindsorContainer();
		_releasePolicy = _container.Kernel.ReleasePolicy;
	}

	[Fact]
	public void AllComponentsReleasePolicy_is_the_default_release_policy_in_Windsor()
	{
		Assert.IsType<LifecycledComponentsReleasePolicy>(_releasePolicy);
	}

	[Fact]
	public void Doesnt_track_simple_components_transient()
	{
		_container.Register(Transient<A>());

		var a = _container.Resolve<A>();

		Assert.False(_releasePolicy.HasTrack(a));
	}

	[Fact]
	public void Doesnt_track_simple_components_with_simple_DynamicDependencies()
	{
		_container.Register(Transient<A>().DynamicParameters(delegate { }));

		var a = _container.Resolve<A>();

		Assert.False(_releasePolicy.HasTrack(a));
	}

	[Fact]
	public void Doesnt_track_simple_components_with_simple_dependencies()
	{
		_container.Register(Transient<B>(),
			Transient<A>());

		var b = _container.Resolve<B>();

		Assert.False(_releasePolicy.HasTrack(b));
	}

	[Fact]
	public void Doesnt_track_simple_components_with_simple_dependencies_having_simple_DynamicDependencies()
	{
		_container.Register(Transient<B>(),
			Transient<A>().DynamicParameters(delegate { }));

		var b = _container.Resolve<B>();

		Assert.False(_releasePolicy.HasTrack(b));
	}

	[Fact]
	public void Doesnt_track_singleton()
	{
		_container.Register(Singleton<A>());

		var a = _container.Resolve<A>();

		Assert.False(_releasePolicy.HasTrack(a));
	}

	[Fact]
	public void Release_doesnt_stop_tracking_component_singleton_until_container_is_disposed()
	{
		DisposableFoo.ResetDisposedCount();
		_container.Register(Singleton<DisposableFoo>());

		var tracker = ReferenceTracker.Track(() => _container.Resolve<DisposableFoo>());

		tracker.AssertStillReferencedAndDo(foo => _container.Release(foo));

		tracker.AssertStillReferenced();
		Assert.Equal(0, DisposableFoo.DisposedCount);

		_container.Dispose();

		tracker.AssertNoLongerReferenced();
		Assert.Equal(1, DisposableFoo.DisposedCount);
	}

	[Fact]
	public void Release_stops_tracking_component_transient()
	{
		_container.Register(Transient<DisposableFoo>());
		var foo = _container.Resolve<DisposableFoo>();

		_container.Release(foo);

		Assert.False(_releasePolicy.HasTrack(foo));
	}

	[Fact]
	public void Tracks_disposable_components()
	{
		_container.Register(Transient<DisposableFoo>());

		var foo = _container.Resolve<DisposableFoo>();

		Assert.True(_releasePolicy.HasTrack(foo));
	}

	[Fact]
	public void Tracks_simple_components_pooled()
	{
		_container.Register(Pooled<A>());

		var a = _container.Resolve<A>();

		Assert.True(_releasePolicy.HasTrack(a));
	}

	[Fact]
	public void Tracks_simple_components_with_DynamicDependencies_requiring_decommission()
	{
		_container.Register(Transient<A>().DynamicParameters((_, _) => delegate { }));

		var a = _container.Resolve<A>();

		Assert.True(_releasePolicy.HasTrack(a));
	}

	[Fact]
	public void Tracks_simple_components_with_disposable_dependencies()
	{
		_container.Register(Transient<DisposableFoo>(),
			Transient<UsesDisposableFoo>());

		var hasFoo = _container.Resolve<UsesDisposableFoo>();

		Assert.True(_releasePolicy.HasTrack(hasFoo));
	}

	[Fact]
	public void Tracks_simple_components_with_simple_dependencies_havingDynamicDependencies_requiring_decommission()
	{
		_container.Register(Transient<B>(),
			Transient<A>().DynamicParameters((_, _) => delegate { }));

		var b = _container.Resolve<B>();

		Assert.True(_releasePolicy.HasTrack(b));
	}

	private ComponentRegistration<T> Pooled<T>()
		where T : class
	{
		return Component.For<T>().LifeStyle.Pooled;
	}

	private ComponentRegistration<T> Singleton<T>()
		where T : class
	{
		return Component.For<T>().LifeStyle.Singleton;
	}

	private ComponentRegistration<T> Transient<T>()
		where T : class
	{
		return Component.For<T>().LifeStyle.Transient;
	}
}