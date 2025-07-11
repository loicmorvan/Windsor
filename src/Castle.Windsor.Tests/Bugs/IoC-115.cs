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

using Castle.MicroKernel.Registration;

namespace Castle.Windsor.Tests.Bugs;

public class IoC115 : AbstractContainerTestCase
{
	[Fact]
	[Bug("IOC-115")]
	public void Can_resolve_from_child_with_dependency_with_dependency_on_parent_component()
	{
		var child = new WindsorContainer();
		Container.AddChildContainer(child);

		Container.Register(Component.For<IParentService>().ImplementedBy<ParentService>());
		child.Register(Component.For<IChildService1>().ImplementedBy<ChildService1>(),
			Component.For<IChildService2>().ImplementedBy<ChildService2>());

		// dependency chain goes ChildService1 --> (I)ChildService2 --> IParentService
		child.Resolve<IChildService1>();
	}

	[Fact]
	[Bug("IOC-115")]
	public void Parent_component_resolved_via_child_container_can_only_depend_on_components_from_parent()
	{
		var child = new WindsorContainer();
		Container.AddChildContainer(child);

		Container.Register(Component.For<IParentService>().ImplementedBy<ParentService>(),
			Component.For<IChildService2>().ImplementedBy<ChildService2>());
		child.Register(Component.For<IParentService>().ImplementedBy<AnotherParentService>());

		var resolve = child.Resolve<IChildService2>();

		Assert.IsType<ParentService>(resolve.Parent);
	}

	public interface IParentService;

	public class ParentService : IParentService;

	public interface IChildService1;

	public class ChildService1 : IChildService1
	{
		public ChildService1(IChildService2 xxx)
		{
		}
	}

	public interface IChildService2
	{
		IParentService Parent { get; }
	}

	public class ChildService2(IParentService xxx) : IChildService2
	{
		public IParentService Parent => xxx;
	}

	public class AnotherParentService : IParentService;
}