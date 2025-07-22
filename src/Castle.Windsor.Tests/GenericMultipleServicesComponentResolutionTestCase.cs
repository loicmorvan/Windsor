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

using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Facilities.TypedFactory.Components;
using Castle.Windsor.Tests.Interceptors;
using Castle.Windsor.Tests.TestImplementationsOfExtensionPoints;

namespace Castle.Windsor.Tests;

public class GenericMultipleServicesComponentResolutionTestCase : AbstractContainerTestCase
{
	protected override void AfterContainerCreated()
	{
		Container.Register(
			Component.For<CountingInterceptor>().LifeStyle.Transient,
			Component.For(typeof(IGeneric<>), typeof(IGenericExtended<>))
				.ImplementedBy(typeof(GenericExtendedImpl<>))
				.Interceptors<CountingInterceptor>(),
			Component.For<UseGenericExtended1>(),
			Component.For<UseGenericExtended2>());
	}


	[Fact]
	public void Can_resolve_generic_component_exposing_interface_and_class_service()
	{
		Container.Register(
			Component.For(typeof(IGeneric<>), typeof(IDummyComponent<>), typeof(GenericDummyComponentImpl<>))
				.ImplementedBy(typeof(GenericDummyComponentImplEx<>)).IsDefault());

		var generic = Container.Resolve<IGeneric<string>>();
		var dummy = Container.Resolve<IDummyComponent<string>>();
		var @class = Container.Resolve<GenericDummyComponentImpl<string>>();

		Assert.Same(generic, dummy);
		Assert.Same(@class, dummy);
	}

	[Fact]
	public void Can_resolve_generic_component_exposing_interface_and_class_service_with_non_generic_base()
	{
		Container.Register(
			Component.For(typeof(IGeneric<>), typeof(IDummyComponent<>))
				.Forward<A, IMarkerInterface>()
				.ImplementedBy(typeof(GenericDummyComponentAImpl<>)).IsDefault());

		var generic = Container.Resolve<IGeneric<string>>();
		var dummy = Container.Resolve<IDummyComponent<string>>();

		Assert.Same(generic, dummy);
		var handler = Kernel.GetHandler(typeof(IGeneric<string>));
		Assert.True(handler.Supports(typeof(A)));
		Assert.True(handler.Supports(typeof(IMarkerInterface)));
	}

	[Fact]
	public void Can_resolve_generic_component_exposing_two_unrelated_implemented_services()
	{
		Container.Register(
			Component.For(typeof(IGeneric<>), typeof(IDummyComponent<>))
				.ImplementedBy(typeof(GenericDummyComponentImpl<>)).IsDefault());

		var generic = Container.Resolve<IGeneric<string>>();
		var dummy = Container.Resolve<IDummyComponent<string>>();

		Assert.Same(generic, dummy);
	}

	[Fact]
	public void Can_resolve_generic_component_exposing_two_unrelated_implemented_services_each_closed_over_different_generic_argument()
	{
		Container.Register(
			Component.For(typeof(IGeneric<>), typeof(IDummyComponent<>))
				.ImplementedBy(typeof(GenericDummyComponentImpl<,>), new DuplicateGenerics()).IsDefault());

		var generic = Container.Resolve<IGeneric<string>>();
		var dummy = Container.Resolve<IDummyComponent<string>>();

		Assert.Same(generic, dummy);
	}

	[Fact]
	public void Dependency_resolution_generic_proxy_should_implement_all_services()
	{
		var comp = Container.Resolve<UseGenericExtended1>();
		Assert.Same(comp.Generic, comp.GenericExtended);
	}

	[Fact]
	public void Generic_handler_caching_should_not_affect_resolution()
	{
		var comp = Container.Resolve<UseGenericExtended2>();
		Assert.Same(comp.Generic, comp.GenericExtended);
	}
}