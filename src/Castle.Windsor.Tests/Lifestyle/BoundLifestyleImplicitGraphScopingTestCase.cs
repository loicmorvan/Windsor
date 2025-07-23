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

using System.Linq;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests.Lifestyle;

public class BoundLifestyleImplicitGraphScopingTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Scoped_component_created_for_outermost_sub_graph()
	{
		Container.Register(
			Component.For<A>().LifeStyle.BoundTo<Cba>(),
			Component.For<B>().LifeStyle.Transient,
			Component.For<Cba>().ImplementedBy<CbaDecorator>().LifeStyle.Transient,
			Component.For<Cba>().LifeStyle.Transient);

		var cba = Container.Resolve<Cba>();
		var inner = ((CbaDecorator)cba).Inner;

		Assert.Same(cba.A, inner.A);
	}

	[Fact]
	public void Scoped_component_disposable_not_tracked()
	{
		Container.Register(
			Component.For<A>().ImplementedBy<ADisposable>().LifestyleBoundTo<Cba>(),
			Component.For<B>().LifeStyle.Transient,
			Component.For<Cba>().LifeStyle.Transient);

		var cba = Container.Resolve<Cba>();

		Assert.False(Kernel.ReleasePolicy.HasTrack(cba.A));
	}

	[Fact]
	public void Scoped_component_disposable_root_tracked()
	{
		Container.Register(
			Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.BoundTo<Cba>(),
			Component.For<B>().LifeStyle.Transient,
			Component.For<Cba>().LifeStyle.Transient);

		var cba = Container.Resolve<Cba>();

		Assert.True(Kernel.ReleasePolicy.HasTrack(cba));
	}

	[Fact]
	public void Scoped_component_doesnt_unnecessarily_force_root_to_be_tracked()
	{
		Container.Register(
			Component.For<A>().LifeStyle.BoundTo<Cba>(),
			Component.For<B>().LifeStyle.Transient,
			Component.For<Cba>().LifeStyle.Transient);

		var cba = Container.Resolve<Cba>();

		Assert.False(Kernel.ReleasePolicy.HasTrack(cba));
		Assert.False(Kernel.ReleasePolicy.HasTrack(cba.B));
	}

	[Fact]
	public void Scoped_component_doesnt_unnecessarily_get_tracked()
	{
		Container.Register(
			Component.For<A>().LifeStyle.BoundTo<Cba>(),
			Component.For<B>().LifeStyle.Transient,
			Component.For<Cba>().LifeStyle.Transient);

		var cba = Container.Resolve<Cba>();

		Assert.False(Kernel.ReleasePolicy.HasTrack(cba.A));
	}

	[Fact]
	public void Scoped_component_not_released_prematurely()
	{
		Container.Register(
			Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.BoundTo<Cba>(),
			Component.For<B>().ImplementedBy<BDisposable>().LifeStyle.Transient,
			Component.For<Cba>().LifeStyle.Transient);

		var cba = Container.Resolve<Cba>();

		var b = (BDisposable)cba.B;
		var wasADisposedAtTheTimeWhenDisposingB = false;
		b.OnDisposing = () => wasADisposedAtTheTimeWhenDisposingB = ((ADisposable)b.A).Disposed;

		Container.Release(cba);

		Assert.True(b.Disposed);
		Assert.False(wasADisposedAtTheTimeWhenDisposingB);
	}

	[Fact]
	public void Scoped_component_not_released_prematurely_interdependencies()
	{
		Container.Register(
			Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.BoundTo<Cba>(),
			Component.For<B>().ImplementedBy<BDisposable>().LifeStyle.BoundTo<Cba>(),
			Component.For<Cba>().LifeStyle.Transient);

		var cba = Container.Resolve<Cba>();

		var b = (BDisposable)cba.B;
		var wasADisposedAtTheTimeWhenDisposingB = false;
		b.OnDisposing = () => wasADisposedAtTheTimeWhenDisposingB = ((ADisposable)b.A).Disposed;

		Container.Release(cba);
		Assert.True(b.Disposed);
		Assert.False(wasADisposedAtTheTimeWhenDisposingB);
	}

	[Fact]
	public void Scoped_component_not_reused_across_resolves()
	{
		Container.Register(
			Component.For<A>().LifeStyle.BoundTo<Cba>(),
			Component.For<B>().LifeStyle.Transient,
			Component.For<Cba>().LifeStyle.Transient);

		var one = Container.Resolve<Cba>();
		var two = Container.Resolve<Cba>();

		Assert.NotSame(one.A, two.A);
		Assert.NotSame(one.B.A, two.B.A);
		Assert.NotSame(one.B.A, two.A);
	}

	[Fact]
	public void Scoped_component_properly_release_when_roots_collection_is_involved()
	{
		Kernel.Resolver.AddSubResolver(new CollectionResolver(Kernel));
		Container.Register(
			Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.BoundTo<AppScreenCba>(),
			Component.For<B>().LifeStyle.Transient,
			Component.For<Cba>().LifeStyle.Transient,
			Component.For<IAppScreen>().ImplementedBy<AppScreenCba>().LifeStyle.Transient.Named("1"),
			Component.For<IAppScreen>().ImplementedBy<AppScreenCba>().LifeStyle.Transient.Named("2"),
			Component.For<IAppScreen>().ImplementedBy<AppScreenCba>().LifeStyle.Transient.Named("3"),
			Component.For<AppHost>().LifeStyle.Transient);

		var host = Container.Resolve<AppHost>();

		var a = host.Screens.Cast<AppScreenCba>().Select(s => s.Dependency.A as ADisposable).ToArray();

		Container.Dispose();

		Assert.True(a.All(x => x.Disposed));
	}

	[Fact]
	public void Scoped_component_properly_scoped_when_roots_collection_is_involved()
	{
		Kernel.Resolver.AddSubResolver(new CollectionResolver(Kernel));
		Container.Register(
			Component.For<A>().LifeStyle.BoundTo<AppScreenCba>(),
			Component.For<B>().LifeStyle.Transient,
			Component.For<Cba>().LifeStyle.Transient,
			Component.For<IAppScreen>().ImplementedBy<AppScreenCba>().LifeStyle.Transient.Named("1"),
			Component.For<IAppScreen>().ImplementedBy<AppScreenCba>().LifeStyle.Transient.Named("2"),
			Component.For<IAppScreen>().ImplementedBy<AppScreenCba>().LifeStyle.Transient.Named("3"),
			Component.For<AppHost>().LifeStyle.Transient);

		var host = Container.Resolve<AppHost>();

		var a = host.Screens.Cast<AppScreenCba>().Select(s => s.Dependency.A).Distinct().ToArray();

		Assert.Equal(3, a.Length);
	}

	[Fact]
	public void Scoped_component_released_when_releasing_root_disposable()
	{
		Container.Register(
			Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.BoundTo<Cba>(),
			Component.For<B>().LifeStyle.Transient,
			Component.For<Cba>().LifeStyle.Transient);

		var cba = Container.Resolve<Cba>();
		var a = (ADisposable)cba.A;

		Container.Release(cba);

		Assert.True(a.Disposed);
	}

	[Fact]
	public void Scoped_component_reused()
	{
		Container.Register(
			Component.For<A>().LifestyleBoundTo<Cba>(),
			Component.For<B>().LifeStyle.Transient,
			Component.For<Cba>().LifeStyle.Transient);

		var cba = Container.Resolve<Cba>();

		Assert.Same(cba.A, cba.B.A);
	}

	[Fact]
	public void Scoped_nearest_component_created_for_innermost_sub_graph()
	{
		Container.Register(
			Component.For<A>().LifeStyle.BoundToNearest<Cba>(),
			Component.For<B>().LifeStyle.Transient,
			Component.For<Cba>().ImplementedBy<CbaDecorator>().LifeStyle.Transient,
			Component.For<Cba>().LifeStyle.Transient);

		var cba = Container.Resolve<Cba>();
		var inner = ((CbaDecorator)cba).Inner;

		Assert.NotSame(cba.A, inner.A);
	}

	[Fact]
	public void Scoped_nearest_component_reused_in_subgraph()
	{
		Container.Register(
			Component.For<A>().LifestyleBoundToNearest<Cba>(),
			Component.For<B>().LifeStyle.Transient,
			Component.For<Cba>().LifeStyle.Transient);

		var cba = Container.Resolve<Cba>();

		Assert.Same(cba.A, cba.B.A);
	}
}