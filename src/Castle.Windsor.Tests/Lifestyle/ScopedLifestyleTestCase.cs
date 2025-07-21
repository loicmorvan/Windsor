// Copyright 2004-2014 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Tests.Lifestyle;

using System;

using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Lifestyle;
using Castle.Windsor.MicroKernel.Lifestyle.Scoped;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Windsor;

public class ScopedLifestyleTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Can_apply_scoped_lifestyle_via_attribute()
	{
		Container.Register(Component.For<ScopedComponent>());

		var handler = Kernel.GetHandler(typeof(ScopedComponent));
		Assert.Equal(LifestyleType.Scoped, handler.ComponentModel.LifestyleType);
	}

	[Fact]
	public void Can_create_scope_without_using_container_or_kernel()
	{
		Container.Register(Component.For<A>().LifeStyle.Scoped());
		using (new CallContextLifetimeScope())
		{
			Container.Resolve<A>();
		}
	}

	[Fact]
	public void Ending_scope_releases_component()
	{
		DisposableFoo.ResetDisposedCount();

		Container.Register(Component.For<DisposableFoo>().LifestyleScoped());

		using (Container.BeginScope())
		{
			Container.Resolve<DisposableFoo>();
		}

		Assert.Equal(1, DisposableFoo.DisposedCount);
	}

	[Fact]
	public void Resolve_scoped_component_within_a_scope_successful()
	{
		Container.Register(Component.For<A>().LifeStyle.Scoped());
		using (Container.BeginScope())
		{
			Container.Resolve<A>();
		}
	}

	[Fact]
	public void Resolve_scoped_component_within_a_scope_successful_registered_via_attribute()
	{
		Container.Register(Component.For<ScopedComponent>());
		using (Container.BeginScope())
		{
			Container.Resolve<ScopedComponent>();
		}
	}

	[Fact]
	public void Resolve_scoped_component_without_a_scope_throws_helpful_exception()
	{
		Container.Register(Component.For<A>().LifeStyle.Scoped());

		var exception = Assert.Throws<InvalidOperationException>(() =>
			Container.Resolve<A>());

		Assert.Equal(
			"Scope was not available. Did you forget to call container.BeginScope()?",
			exception.Message);
	}

	[Fact]
	public void Scoped_component_instance_is_reused_within_the_scope()
	{
		Container.Register(Component.For<A>().LifeStyle.Scoped());

		using (Container.BeginScope())
		{
			var a1 = Container.Resolve<A>();
			var a2 = Container.Resolve<A>();
			Assert.Same(a1, a2);
		}
	}

	[Fact]
	public void Scoped_component_is_bound_to_the_innermost_scope()
	{
		DisposableFoo.ResetDisposedCount();

		Container.Register(Component.For<DisposableFoo>().LifeStyle.Scoped());

		using (Container.BeginScope())
		{
			using (Container.BeginScope())
			{
				Container.Resolve<DisposableFoo>();
				Assert.Equal(0, DisposableFoo.DisposedCount);
			}

			Assert.Equal(1, DisposableFoo.DisposedCount);
		}

		Assert.Equal(1, DisposableFoo.DisposedCount);
	}

	[Fact]
	public void Scoped_component_is_not_released_by_call_to_container_Release()
	{
		DisposableFoo foo;
		DisposableFoo.ResetDisposedCount();

		Container.Register(Component.For<DisposableFoo>().LifeStyle.Scoped());

		using (Container.BeginScope())
		{
			foo = Container.Resolve<DisposableFoo>();
			Container.Release(foo);
			Assert.Equal(0, DisposableFoo.DisposedCount);
		}
	}

	[Fact]
	public void Scoped_component_is_not_tracked_by_the_release_policy()
	{
		DisposableFoo foo;
		DisposableFoo.ResetDisposedCount();

		Container.Register(Component.For<DisposableFoo>().LifeStyle.Scoped());

		using (Container.BeginScope())
		{
			foo = Container.Resolve<DisposableFoo>();
			Assert.False(Kernel.ReleasePolicy.HasTrack(foo));
		}
	}

	[Fact]
	public void Transient_depending_on_scoped_component_is_not_tracked_by_the_container()
	{
		Container.Register(Component.For<DisposableFoo>().LifeStyle.Scoped(),
			Component.For<UsesDisposableFoo>().LifeStyle.Transient);

		using (Container.BeginScope())
		{
			ReferenceTracker
				.Track(() => Container.Resolve<UsesDisposableFoo>())
				.AssertNoLongerReferenced();
		}
	}

	[Fact]
	public void Transient_depending_on_scoped_component_is_not_tracked_by_the_release_policy()
	{
		Container.Register(Component.For<DisposableFoo>().LifeStyle.Scoped(),
			Component.For<UsesDisposableFoo>().LifeStyle.Transient);

		using (Container.BeginScope())
		{
			var udf = Container.Resolve<UsesDisposableFoo>();
			Assert.False(Kernel.ReleasePolicy.HasTrack(udf));
		}
	}

	[Fact]
	public void Requiring_scope_without_parent_scope_begins_new_scope()
	{
		Container.Register(Component.For<A>().LifeStyle.Scoped());
		using var scope = Container.RequireScope();
		Container.Resolve<A>();
		Assert.NotNull(scope);
	}

	[Fact]
	public void Requiring_scope_within_parent_scope_uses_parent_scope()
	{
		Container.Register(Component.For<A>().LifeStyle.Scoped());
		using (Container.BeginScope())
		{
			var a = Container.Resolve<A>();
			using (var scope = Container.RequireScope())
			{
				var aa = Container.Resolve<A>();
				Assert.Same(a, aa);
				Assert.Null(scope);
			}
		}
	}

	[Fact]
	[Bug("IOC-319")]
	public void Nested_container_and_scope_used_together_dont_cause_components_to_be_released_twice()
	{
		DisposableFoo.ResetDisposedCount();
		Container.Register(Component.For<IWindsorContainer>().LifeStyle.Scoped()
			.UsingFactoryMethod(k =>
			{
				var container = new WindsorContainer();
				container.Register(Component.For<DisposableFoo>().LifestyleScoped());

				k.AddChildKernel(container.Kernel);
				return container;
			}));
		using (Container.BeginScope())
		{
			var child = Container.Resolve<IWindsorContainer>();
			child.Resolve<DisposableFoo>();
		}

		Assert.Equal(1, DisposableFoo.DisposedCount);
	}
}