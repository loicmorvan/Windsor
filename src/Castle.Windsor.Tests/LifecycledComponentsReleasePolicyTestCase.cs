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
	private readonly IWindsorContainer container;
	private readonly IReleasePolicy releasePolicy;

	public LifecycledComponentsReleasePolicyTestCase()
	{
		container = new WindsorContainer();
		releasePolicy = container.Kernel.ReleasePolicy;
	}

	[Fact]
	public void AllComponentsReleasePolicy_is_the_default_release_policy_in_Windsor()
	{
		Assert.IsType<LifecycledComponentsReleasePolicy>(releasePolicy);
	}

	[Fact]
	public void Doesnt_track_simple_components_transient()
	{
		container.Register(Transient<A>());

		var a = container.Resolve<A>();

		Assert.False(releasePolicy.HasTrack(a));
	}

	[Fact]
	public void Doesnt_track_simple_components_with_simple_DynamicDependencies()
	{
		container.Register(Transient<A>().DynamicParameters(delegate { }));

		var a = container.Resolve<A>();

		Assert.False(releasePolicy.HasTrack(a));
	}

	[Fact]
	public void Doesnt_track_simple_components_with_simple_dependencies()
	{
		container.Register(Transient<B>(),
			Transient<A>());

		var b = container.Resolve<B>();

		Assert.False(releasePolicy.HasTrack(b));
	}

	[Fact]
	public void Doesnt_track_simple_components_with_simple_dependencies_having_simple_DynamicDependencies()
	{
		container.Register(Transient<B>(),
			Transient<A>().DynamicParameters(delegate { }));

		var b = container.Resolve<B>();

		Assert.False(releasePolicy.HasTrack(b));
	}

	[Fact]
	public void Doesnt_track_singleton()
	{
		container.Register(Singleton<A>());

		var a = container.Resolve<A>();

		Assert.False(releasePolicy.HasTrack(a));
	}

	[Fact]
	public void Release_doesnt_stop_tracking_component_singleton_until_container_is_disposed()
	{
		DisposableFoo.ResetDisposedCount();
		container.Register(Singleton<DisposableFoo>());

		var tracker = ReferenceTracker.Track(() => container.Resolve<DisposableFoo>());

		tracker.AssertStillReferencedAndDo(foo => container.Release(foo));

		tracker.AssertStillReferenced();
		Assert.Equal(0, DisposableFoo.DisposedCount);

		container.Dispose();

		tracker.AssertNoLongerReferenced();
		Assert.Equal(1, DisposableFoo.DisposedCount);
	}

	[Fact]
	public void Release_stops_tracking_component_transient()
	{
		container.Register(Transient<DisposableFoo>());
		var foo = container.Resolve<DisposableFoo>();

		container.Release(foo);

		Assert.False(releasePolicy.HasTrack(foo));
	}

	[Fact]
	public void Tracks_disposable_components()
	{
		container.Register(Transient<DisposableFoo>());

		var foo = container.Resolve<DisposableFoo>();

		Assert.True(releasePolicy.HasTrack(foo));
	}

	[Fact]
	public void Tracks_simple_components_pooled()
	{
		container.Register(Pooled<A>());

		var a = container.Resolve<A>();

		Assert.True(releasePolicy.HasTrack(a));
	}

	[Fact]
	public void Tracks_simple_components_with_DynamicDependencies_requiring_decommission()
	{
		container.Register(Transient<A>().DynamicParameters((_, _) => delegate { }));

		var a = container.Resolve<A>();

		Assert.True(releasePolicy.HasTrack(a));
	}

	[Fact]
	public void Tracks_simple_components_with_disposable_dependencies()
	{
		container.Register(Transient<DisposableFoo>(),
			Transient<UsesDisposableFoo>());

		var hasFoo = container.Resolve<UsesDisposableFoo>();

		Assert.True(releasePolicy.HasTrack(hasFoo));
	}

	[Fact]
	public void Tracks_simple_components_with_simple_dependencies_havingDynamicDependencies_requiring_decommission()
	{
		container.Register(Transient<B>(),
			Transient<A>().DynamicParameters((_, _) => delegate { }));

		var b = container.Resolve<B>();

		Assert.True(releasePolicy.HasTrack(b));
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