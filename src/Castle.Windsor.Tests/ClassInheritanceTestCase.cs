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
using System.Linq;
using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Interceptors;

namespace Castle.Windsor.Tests;

public class ClassInheritanceTestCase : AbstractContainerTestCase
{
	// TODO: add tests for generics in the hierarchy (open as well?)
	private bool IsProxy(object o)
	{
		return o is IProxyTargetAccessor;
	}

	private void RegisterInterceptor()
	{
		Container.Register(Component.For<CountingInterceptor>().LifeStyle.Transient);
	}

	[Fact]
	public void Can_proxy_class_service_impl_explicitly()
	{
		RegisterInterceptor();
		Container.Register(Component.For<JohnChild>().ImplementedBy<JohnChild>().LifeStyle.Transient
			.Interceptors<CountingInterceptor>());

		var child = Container.Resolve<JohnChild>();

		Assert.True(IsProxy(child));
	}

	[Fact]
	public void Can_proxy_class_service_impl_implicitly()
	{
		RegisterInterceptor();
		Container.Register(Component.For<JohnChild>().LifeStyle.Transient.Interceptors<CountingInterceptor>());

		var child = Container.Resolve<JohnChild>();

		Assert.True(IsProxy(child));
	}

	[Fact]
	public void Can_proxy_class_service_with_inherited_implementation()
	{
		RegisterInterceptor();
		Container.Register(Component.For<JohnParent>().ImplementedBy<JohnChild>().LifeStyle.Transient
			.Interceptors<CountingInterceptor>());

		var obj = Container.Resolve<JohnParent>();

		Assert.True(IsProxy(obj));
		Assert.IsType<JohnChild>(obj);
	}

	[Fact]
	public void Can_proxy_multiple_class_services_and_interface_with_inherited_implementation()
	{
		RegisterInterceptor();
		Container.Register(Component.For<JohnParent, IEmptyService, JohnGrandparent>().ImplementedBy<JohnChild>()
			.LifeStyle.Transient.Interceptors<CountingInterceptor>());

		var obj = Container.Resolve<JohnParent>();

		Assert.True(IsProxy(obj));
		Assert.IsType<JohnChild>(obj);
		Assert.IsType<IEmptyService>(obj, false);
	}

	[Fact]
	public void Can_proxy_multiple_class_services_and_interfaces_incl_generic_with_inherited_implementation()
	{
		RegisterInterceptor();
		Container.Register(Component.For<IGeneric<IEmployee>, JohnParent, IEmptyService, JohnGrandparent>()
			.ImplementedBy(typeof(JohnChild))
			.LifeStyle.Transient.Interceptors<CountingInterceptor>());

		var obj = Container.Resolve<JohnParent>();

		Assert.True(IsProxy(obj));
		Assert.IsType<JohnChild>(obj);
		Assert.IsType<IEmptyService>(obj, false);
		Assert.IsType<IGeneric<IEmployee>>(obj, false);
	}

	[Fact]
	public void Can_proxy_multiple_class_services_with_inherited_implementation()
	{
		RegisterInterceptor();
		Container.Register(Component.For<JohnParent, JohnGrandparent>().ImplementedBy<JohnChild>().LifeStyle.Transient
			.Interceptors<CountingInterceptor>());

		var obj = Container.Resolve<JohnParent>();

		Assert.True(IsProxy(obj));
		Assert.IsType<JohnChild>(obj);
	}

	[Fact]
	public void GrandParent_and_Parent_of_impl_can_be_the_service()
	{
		Container.Register(Component.For<JohnGrandparent, JohnParent>().ImplementedBy<JohnChild>());

		var grandparent = Container.Resolve<JohnGrandparent>();
		var parent = Container.Resolve<JohnParent>();

		Assert.Same(grandparent, parent);
		Assert.IsType<JohnChild>(grandparent);
	}

	[Fact]
	public void GrandParent_of_impl_can_be_the_service()
	{
		Container.Register(Component.For<JohnGrandparent>().ImplementedBy<JohnChild>());

		var grandparent = Container.Resolve<JohnGrandparent>();

		Assert.IsType<JohnChild>(grandparent);
	}

	[Fact]
	public void Not_related_service_and_impl_fail_on_resolve()
	{
		Container.Register(Component.For<A>().ImplementedBy(typeof(A2)));

		var handler = Kernel.GetHandler(typeof(A));

		Assert.Equal(typeof(A), handler.ComponentModel.Services.Single());
		Assert.Equal(typeof(A2), handler.ComponentModel.Implementation);
		// sure, why not - let them do uncompatible types. Who knows - perhaps by some miracle
		Assert.Throws<InvalidCastException>(() => Container.Resolve<A>());
	}

	[Fact]
	public void Parent_and_GrandParent_of_impl_can_be_the_service()
	{
		Container.Register(Component.For<JohnParent, JohnGrandparent>().ImplementedBy<JohnChild>());

		var grandparent = Container.Resolve<JohnGrandparent>();
		var parent = Container.Resolve<JohnParent>();

		Assert.Same(grandparent, parent);
		Assert.IsType<JohnChild>(grandparent);
	}

	[Fact]
	public void Parent_of_impl_can_be_the_service()
	{
		Container.Register(Component.For<JohnParent>().ImplementedBy<JohnChild>());

		var parent = Container.Resolve<JohnParent>();

		Assert.IsType<JohnChild>(parent);
	}

	[Fact]
	public void Same_class_can_be_used_as_service_and_impl_explicitly()
	{
		Container.Register(Component.For<A>().ImplementedBy<A>());

		var handler = Kernel.GetHandler(typeof(A));

		Assert.Equal(typeof(A), handler.ComponentModel.Services.Single());
		Assert.Equal(typeof(A), handler.ComponentModel.Implementation);
	}

	[Fact]
	public void Same_class_can_be_used_as_service_and_impl_implicitly()
	{
		Container.Register(Component.For<A>());

		var handler = Kernel.GetHandler(typeof(A));

		Assert.Equal(typeof(A), handler.ComponentModel.Services.Single());
		Assert.Equal(typeof(A), handler.ComponentModel.Implementation);
	}
}