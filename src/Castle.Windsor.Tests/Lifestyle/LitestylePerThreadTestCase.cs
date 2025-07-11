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

using System;
using System.Threading;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests.Lifestyle;

public class LitestylePerThreadTestCase : AbstractContainerTestCase
{
	private void ExecuteOnAnotherThreadAndWait(Action action)
	{
		var @event = new ManualResetEvent(false);
		new Thread(() =>
		{
			action.Invoke();
			@event.Set();
		}).Start();
		@event.WaitOne();
	}

	[Fact]
	public void Disposable_components_are_decommissioned_on_container_Dispose()
	{
		Container.Register(Component.For<DisposableComponent>().LifestylePerThread());
		var a = Container.Resolve<DisposableComponent>();
		Container.Dispose();
		Assert.True(a.Disposed);
	}

	[Fact]
	public void Disposable_components_are_decommissioned_on_container_Dispose_all_threads()
	{
		Container.Register(Component.For<DisposableComponent>().LifeStyle.PerThread);
		var a1 = Container.Resolve<DisposableComponent>();
		DisposableComponent a2 = null;
		ExecuteOnAnotherThreadAndWait(() => a2 = Container.Resolve<DisposableComponent>());

		Container.Dispose();

		Assert.True(a1.Disposed);
		Assert.True(a2.Disposed);
	}

	[Fact]
	public void Disposable_components_are_not_decommissioned_on_Release_call()
	{
		Container.Register(Component.For<DisposableComponent>().LifeStyle.PerThread);
		var a = Container.Resolve<DisposableComponent>();
		Container.Release(a);
		Assert.False(a.Disposed);
	}

	[Fact]
	public void Instances_created_on_different_threads_are_not_reused()
	{
		Container.Register(Component.For<A>().LifeStyle.PerThread);
		var a1 = Container.Resolve<A>();
		A a2 = null;
		ExecuteOnAnotherThreadAndWait(() => a2 = Container.Resolve<A>());

		Assert.NotSame(a1, a2);
	}

	[Fact]
	public void Instances_created_on_the_same_thread_are_reused()
	{
		Container.Register(Component.For<A>().LifeStyle.PerThread);
		var a1 = Container.Resolve<A>();
		var a2 = Container.Resolve<A>();
		Assert.Same(a1, a2);
	}

	[Fact]
	public void Instances_created_on_the_same_thread_are_reused_in_child_container()
	{
		Container.Register(Component.For<A>().LifeStyle.PerThread);
		var a1 = Container.Resolve<A>();
		var child = new WindsorContainer();
		Container.AddChildContainer(child);
		var a2 = child.Resolve<A>();
		Assert.Same(a1, a2);
	}
}