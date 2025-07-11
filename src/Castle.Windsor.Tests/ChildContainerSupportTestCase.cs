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

using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

public class ChildContainerSupportTestCase : AbstractContainerTestCase
{
	[Fact]
	[Bug("IOC-127")]
	public void AddComponentInstanceAndChildContainers()
	{
		var child = new WindsorContainer();
		Container.AddChildContainer(child);

		var clock1 = new EmptyServiceA();
		var clock2 = new EmptyServiceB();

		Container.Register(Component.For<IEmptyService>().Instance(clock2));
		child.Register(Component.For<IEmptyService>().Instance(clock1));

		Assert.Same(clock2, Container.Resolve<IEmptyService>());
		Assert.Same(clock1, child.Resolve<IEmptyService>());
	}

	[Fact]
	public void AddAndRemoveChildContainer()
	{
		IWindsorContainer childcontainer = new WindsorContainer();
		Container.AddChildContainer(childcontainer);
		Assert.Equal(Container, childcontainer.Parent);

		Container.RemoveChildContainer(childcontainer);
		Assert.Null(childcontainer.Parent);

		Container.AddChildContainer(childcontainer);
		Assert.Equal(Container, childcontainer.Parent);
	}

	[Fact]
	public void AddAndRemoveChildContainerWithProperty()
	{
		IWindsorContainer childcontainer = new WindsorContainer();
		childcontainer.Parent = Container;
		Assert.Equal(Container, childcontainer.Parent);

		childcontainer.Parent = null;
		Assert.Null(childcontainer.Parent);

		childcontainer.Parent = Container;
		Assert.Equal(Container, childcontainer.Parent);
	}

	[Fact]
	public void AddingToTwoParentContainsThrowsKernelException()
	{
		IWindsorContainer container3 = new WindsorContainer();
		IWindsorContainer childcontainer = new WindsorContainer();
		Container.AddChildContainer(childcontainer);
		Assert.Throws<KernelException>(() => container3.AddChildContainer(childcontainer));
	}

	[Fact]
	public void AddingToTwoParentWithPropertyContainsThrowsKernelException()
	{
		IWindsorContainer container3 = new WindsorContainer();
		IWindsorContainer childcontainer = new WindsorContainer();
		childcontainer.Parent = Container;
		Assert.Throws<KernelException>(() => childcontainer.Parent = container3);
	}

	protected override void AfterContainerCreated()
	{
		Container.Register(Component.For(typeof(A)).Named("A"));
	}

	[Fact]
	public void ResolveAgainstParentContainer()
	{
		IWindsorContainer childcontainer = new WindsorContainer();
		Container.AddChildContainer(childcontainer);

		Assert.Equal(Container, childcontainer.Parent);

		childcontainer.Register(Component.For(typeof(B)).Named("B"));
		var b = childcontainer.Resolve<B>("B");
		Assert.NotNull(b);
	}

	[Fact]
	public void ResolveAgainstParentContainerWithProperty()
	{
		IWindsorContainer childcontainer = new WindsorContainer { Parent = Container };

		Assert.Equal(Container, childcontainer.Parent);

		childcontainer.Register(Component.For(typeof(B)).Named("B"));
		var b = childcontainer.Resolve<B>("B");

		Assert.NotNull(b);
	}

#if FEATURE_SYSTEM_CONFIGURATION
		[Fact]
		public void StartWithParentContainer()
		{
			IWindsorContainer childcontainer = new WindsorContainer(Container, new XmlInterpreter());

			Assert.Equal(Container, childcontainer.Parent);

			childcontainer.Register(Component.For(typeof(B)).Named("B"));
			var b = childcontainer.Resolve<B>("B");

			Assert.NotNull(b);
		}
#endif
}