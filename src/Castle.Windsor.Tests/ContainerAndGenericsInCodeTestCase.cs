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

namespace Castle.Windsor.Tests;

using Castle.DynamicProxy;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Interceptors;

public class ContainerAndGenericsInCodeTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Can_create_generic_with_ctor_dependency_on_array_of_generics()
	{
		Kernel.Resolver.AddSubResolver(new CollectionResolver(Kernel));
		Container.Register(Component.For(typeof(UsesArrayOfGeneric<>)),
			Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl1<>)));

		Container.Resolve<UsesArrayOfGeneric<int>>();
	}

	[Fact]
	public void Can_create_nonGeneric_with_ctor_dependency_on_generic()
	{
		Container.Register(Component.For<NeedsGenericType>(),
			Component.For(typeof(ICache<>)).ImplementedBy(typeof(NullCache<>)));

		var needsGenericType = Container.Resolve<NeedsGenericType>();

		Assert.NotNull(needsGenericType);
	}

	[Fact]
	public void Can_intercept_open_generic_components()
	{
		Container.Register(Component.For<CollectInterceptedIdInterceptor>(),
			Component.For(typeof(Components.IRepository<>)).ImplementedBy(typeof(DemoRepository<>))
				.Interceptors<CollectInterceptedIdInterceptor>());

		var demoRepository = Container.Resolve<Components.IRepository<object>>();
		demoRepository.Get(12);

		Assert.Equal(12, CollectInterceptedIdInterceptor.InterceptedId);
	}

	[Fact]
	public void Can_proxy_closed_generic_components()
	{
		Container.AddFacility<MyInterceptorGreedyFacility>();
		Container.Register(Component.For<StandardInterceptor>().Named("interceptor"),
			Component.For<Components.IRepository<Employee>>()
				.ImplementedBy<DemoRepository<Employee>>()
				.Named("key"));

		var store = Container.Resolve<Components.IRepository<Employee>>();

		Assert.IsNotType<DemoRepository<Employee>>(store);
	}

	[Fact]
	public void Can_proxy_open_generic_components()
	{
		Container.AddFacility<MyInterceptorGreedyFacility2>();
		Container.Register(Component.For<StandardInterceptor>().Named("interceptor"),
			Component.For(typeof(Components.IRepository<>)).ImplementedBy(typeof(DemoRepository<>)));

		var store = Container.Resolve<Components.IRepository<Employee>>();

		Assert.IsNotType<DemoRepository<Employee>>(store);
	}

	[Fact]
	public void Open_generic_singleton_produces_unique_instances_per_closed_type()
	{
		Container.Register(
			Component.For(typeof(Components.IRepository<>))
				.ImplementedBy(typeof(RepositoryNotMarkedAsTransient<>))
				.LifeStyle.Singleton);

		var o1 = Container.Resolve<Components.IRepository<Employee>>();
		var o2 = Container.Resolve<Components.IRepository<Employee>>();
		var o3 = Container.Resolve<Components.IRepository<Reviewer>>();
		var o4 = Container.Resolve<Components.IRepository<Reviewer>>();

		Assert.Same(o1, o2);
		Assert.Same(o3, o4);
		Assert.NotSame(o1, o4);
	}

	[Fact]
	public void Open_generic_trasient_via_attribute_produces_unique_instances()
	{
		Container.Register(Component.For(typeof(Components.IRepository<>))
			.ImplementedBy(typeof(TransientRepository<>)));

		var o1 = Container.Resolve<Components.IRepository<Employee>>();
		var o2 = Container.Resolve<Components.IRepository<Employee>>();
		var o3 = Container.Resolve<Components.IRepository<Reviewer>>();
		var o4 = Container.Resolve<Components.IRepository<Reviewer>>();

		Assert.NotSame(o1, o2);
		Assert.NotSame(o1, o3);
		Assert.NotSame(o1, o4);
	}

	[Fact]
	public void Open_generic_trasient_via_registration_produces_unique_instances()
	{
		Container.Register(
			Component.For(typeof(Components.IRepository<>))
				.ImplementedBy(typeof(RepositoryNotMarkedAsTransient<>))
				.LifeStyle.Transient);

		var o1 = Container.Resolve<Components.IRepository<Employee>>();
		var o2 = Container.Resolve<Components.IRepository<Employee>>();
		var o3 = Container.Resolve<Components.IRepository<Reviewer>>();
		var o4 = Container.Resolve<Components.IRepository<Reviewer>>();

		Assert.NotSame(o1, o2);
		Assert.NotSame(o1, o3);
		Assert.NotSame(o1, o4);
	}

	[Fact]
	public void Proxy_for_generic_component_does_not_affect_lifestyle()
	{
		Container.AddFacility<MyInterceptorGreedyFacility2>();
		Container.Register(Component.For<StandardInterceptor>().Named("interceptor"),
			Component.For(typeof(Components.IRepository<>))
				.ImplementedBy(typeof(DemoRepository<>))
				.LifeStyle.Transient);

		var store = Container.Resolve<Components.IRepository<Employee>>();
		var anotherStore = Container.Resolve<Components.IRepository<Employee>>();

		Assert.IsNotType<DemoRepository<Employee>>(store);
		Assert.IsNotType<DemoRepository<Employee>>(anotherStore);
		Assert.NotSame(store, anotherStore);
	}

	[Fact]
	public void Proxy_parent_does_not_make_generic_child_a_proxy()
	{
		Container.Register(Component.For<CollectInterceptedIdInterceptor>(),
			Component.For<ISpecification>()
				.ImplementedBy<MySpecification>()
				.Interceptors<CollectInterceptedIdInterceptor>(),
			Component.For(typeof(Components.IRepository<>))
				.ImplementedBy(typeof(TransientRepository<>))
				.Named("repos"));

		var specification = Container.Resolve<ISpecification>();

		Assert.IsType<TransientRepository<int>>(specification.Repository);
	}
}