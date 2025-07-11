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

using System;
using System.Linq;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Releasers;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Facilities.TypedFactory.Components;
using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;
using Castle.Windsor.Tests.Facilities.TypedFactory.Selectors;
using Castle.Windsor.Tests.Interceptors;

namespace Castle.Windsor.Tests.Facilities.TypedFactory;

public class TypedFactoryFacilityTake2TestCase : AbstractContainerTestCase
{
	protected override void AfterContainerCreated()
	{
		Container.AddFacility<TypedFactoryFacility>();
		Container.Register(Component.For<IDummyComponent>().ImplementedBy<Component1>().LifestyleTransient());
	}

	[Fact]
	public void Can_Resolve_by_closed_generic_closed_on_arguments_type_with_custom_selector()
	{
		Container.Register(Classes.FromAssemblyContaining<TypedFactoryFacilityTake2TestCase>()
				.BasedOn(typeof(GenericComponent<>))
				.WithService.Base().Configure(c => c.LifestyleTransient()),
			Component.For<IObjectFactory>().AsFactory(s => s.SelectedWith<SelectorByClosedArgumentType>()),
			Component.For<SelectorByClosedArgumentType>());

		var factory = Container.Resolve<IObjectFactory>();

		var one = factory.GetItemByWithParameter(3);
		var two = factory.GetItemByWithParameter("two");
		Assert.IsType<GenericIntComponent>(one);
		Assert.IsType<GenericStringComponent>(two);

		Assert.Equal(3, ((GenericIntComponent)one).Value);
		Assert.Equal("two", ((GenericStringComponent)two).Value);
	}

	[Fact]
	public void Can_Resolve_by_name_with_custom_selector()
	{
		Container.Register(
			Component.For<IDummyComponent>()
				.ImplementedBy<Component1>()
				.Named("one"),
			Component.For<IDummyComponent>()
				.ImplementedBy<Component2>()
				.Named("two"),
			Component.For<IFactoryById>().AsFactory(f => f.SelectedWith<SelectorById>()),
			Component.For<SelectorById>());

		var factory = Container.Resolve<IFactoryById>();

		var one = factory.ComponentNamed("one");
		var two = factory.ComponentNamed("two");
		Assert.IsType<Component1>(one);
		Assert.IsType<Component2>(two);
	}

	[Fact]
	public void Can_Resolve_multiple_components_at_once_with_default_selector_list()
	{
		Container.Register(
			Component.For<IDummyComponent>()
				.ImplementedBy<Component2>()
				.LifeStyle.Transient,
			Component.For<IDummyComponentListFactory>()
				.AsFactory());
		var factory = Container.Resolve<IDummyComponentListFactory>();

		var all = factory.All();
		Assert.NotNull(all);
		Assert.Equal(2, all.Count);
		Assert.Contains(all, c => c is Component1);
		Assert.Contains(all, c => c is Component2);
	}

	[Fact]
	public void Can_Resolve_multiple_components_at_once_with_non_default_selector_array()
	{
		Container.Register(
			Component.For<IDummyComponent>()
				.ImplementedBy<Component2>()
				.LifeStyle.Transient,
			Component.For<IDummyComponentArrayFactory>()
				.AsFactory(),
			Component.For<ITypedFactoryComponentSelector>()
				.ImplementedBy<MultipleSelector>());
		var factory = Container.Resolve<IDummyComponentArrayFactory>();

		var all = factory.All();
		Assert.NotNull(all);
		Assert.Equal(2, all.Length);
		Assert.Contains(all, c => c is Component1);
		Assert.Contains(all, c => c is Component2);
	}

	[Fact]
	public void Can_Resolve_multiple_components_at_once_with_non_default_selector_collection()
	{
		Container.Register(
			Component.For<IDummyComponent>()
				.ImplementedBy<Component2>()
				.LifeStyle.Transient,
			Component.For<IDummyComponentCollectionFactory>()
				.AsFactory(),
			Component.For<ITypedFactoryComponentSelector>()
				.ImplementedBy<MultipleSelector>());
		var factory = Container.Resolve<IDummyComponentCollectionFactory>();

		var all = factory.All().ToArray();
		Assert.NotNull(all);
		Assert.Equal(2, all.Length);
		Assert.Contains(all, c => c is Component1);
		Assert.Contains(all, c => c is Component2);
	}

	[Fact]
	public void Can_Resolve_multiple_components_at_once_with_non_default_selector_enumerable()
	{
		Container.Register(
			Component.For<IDummyComponent>()
				.ImplementedBy<Component2>()
				.LifeStyle.Transient,
			Component.For<IDummyComponentEnumerableFactory>()
				.AsFactory(),
			Component.For<ITypedFactoryComponentSelector>()
				.ImplementedBy<MultipleSelector>());
		var factory = Container.Resolve<IDummyComponentEnumerableFactory>();

		var all = factory.All().ToArray();
		Assert.NotNull(all);
		Assert.Equal(2, all.Length);
		Assert.Contains(all, c => c is Component1);
		Assert.Contains(all, c => c is Component2);
	}

	[Fact]
	public void Can_Resolve_multiple_components_at_once_with_non_default_selector_list()
	{
		Container.Register(
			Component.For<IDummyComponent>()
				.ImplementedBy<Component2>()
				.LifeStyle.Transient,
			Component.For<IDummyComponentListFactory>()
				.AsFactory(),
			Component.For<ITypedFactoryComponentSelector>()
				.ImplementedBy<MultipleSelector>());
		var factory = Container.Resolve<IDummyComponentListFactory>();

		var all = factory.All();
		Assert.NotNull(all);
		Assert.Equal(2, all.Count);
		Assert.Contains(all, c => c is Component1);
		Assert.Contains(all, c => c is Component2);
	}

	[Fact]
	public void Can_resolve_component()
	{
		Container.Register(Component.For<IDummyComponentFactory>().AsFactory());
		var factory = Container.Resolve<IDummyComponentFactory>();

		var component = factory.CreateDummyComponent();
		Assert.NotNull(component);
	}

	[Fact]
	public void Can_resolve_component_by_name_with_default_selector()
	{
		Container.Register(
			Component.For<IDummyComponent>()
				.ImplementedBy<Component2>()
				.Named("SecondComponent")
				.LifeStyle.Transient,
			Component.For<IDummyComponentFactory>()
				.AsFactory());
		var factory = Container.Resolve<IDummyComponentFactory>();

		var component = factory.GetSecondComponent();
		Assert.NotNull(component);
		Assert.IsType<Component2>(component);
	}

	[Fact]
	public void Can_resolve_open_generic_components()
	{
		Container.Register(
			Component.For<IGenericComponentsFactory>().AsFactory(),
			Component.For(typeof(GenericComponentWithIntArg<>)).LifestyleSingleton(),
			Component.For(typeof(GenericComponent<>)).LifestyleSingleton());

		var factory = Container.Resolve<IGenericComponentsFactory>();

		factory.CreateGeneric<GenericComponent<int>>();
		factory.CreateGeneric<GenericComponent<IDisposable>>();

		var component = factory.CreateGeneric<GenericComponentWithIntArg<string>, int>(667);
		Assert.Equal(667, component.Property);
	}

	[Fact]
	public void Can_resolve_via_factory_with_generic_method()
	{
		Container.Register(
			Component.For<IGenericComponentsFactory>().AsFactory());

		var factory = Container.Resolve<IGenericComponentsFactory>();
		var component = factory.CreateGeneric<IDummyComponent>();
		Assert.IsType<Component1>(component);
	}

	[Fact]
	public void Can_resolve_via_generic_factory_closed()
	{
		Container.Register(Component.For<IGenericFactoryClosed>().AsFactory());

		var factory = Container.Resolve<IGenericFactoryClosed>();

		var component = factory.Create();
		Assert.NotNull(component);
	}

	[Fact]
	public void Can_resolve_via_generic_factory_closed_doubly()
	{
		Container.Register(Component.For<IGenericFactoryClosedDoubly>().AsFactory());

		var factory = Container.Resolve<IGenericFactoryClosedDoubly>() as IGenericFactory<IDummyComponent>;

		var component = factory.Create();
		Assert.NotNull(component);
	}

	[Fact]
	public void Can_resolve_via_generic_factory_inherited_semi_closing()
	{
		Container.Register(Component.For(typeof(IGenericFactoryDouble<,>)).AsFactory(),
			Component.For<IProtocolHandler>().ImplementedBy<MirandaProtocolHandler>().LifeStyle.Transient);

		var factory = Container.Resolve<IGenericFactoryDouble<IDummyComponent, IProtocolHandler>>();

		factory.Create();
		factory.Create2();
	}

	[Fact]
	public void Can_resolve_via_generic_factory_with_generic_method()
	{
		Container.Register(
			Component.For(typeof(IDummyComponent<>)).ImplementedBy(typeof(DummyComponent<>)).LifeStyle.Transient,
			Component.For(typeof(IGenericFactoryWithGenericMethod<>)).AsFactory());

		var factory = Container.Resolve<IGenericFactoryWithGenericMethod<A>>();
		factory.Create<IDummyComponent<A>>();
	}

	[Fact]
	public void Can_use_additional_interceptors_on_interface_based_factory()
	{
		Container.Register(
			Component.For<CollectInvocationsInterceptor>(),
			Component.For<IDummyComponentFactory>().Interceptors<CollectInvocationsInterceptor>().AsFactory());
		var factory = Container.Resolve<IDummyComponentFactory>();

		var component = factory.CreateDummyComponent();
		Assert.NotNull(component);

		var interceptor = Container.Resolve<CollectInvocationsInterceptor>();

		Assert.Single(interceptor.Invocations);
		Assert.Same(component, interceptor.Invocations[0].ReturnValue);
	}

	[Fact]
	public void Can_use_non_default_selector()
	{
		Container.Register(
			Component.For<IDummyComponent>()
				.ImplementedBy<Component2>()
				.Named("foo")
				.LifeStyle.Transient,
			Component.For<IDummyComponentFactory>()
				.AsFactory(f => f.SelectedWith<FooSelector>()),
			Component.For<FooSelector>());
		var factory = Container.Resolve<IDummyComponentFactory>();

		var component = factory.GetSecondComponent();
		Assert.IsType<Component2>(component);

		component = factory.CreateDummyComponent();
		Assert.IsType<Component2>(component);
	}

	[Fact]
	public void Component_released_out_of_band_is_STILL_tracked()
	{
		Container.Register(
			Component.For<INonDisposableFactory>().LifeStyle.Transient.AsFactory(),
			Component.For<DisposableComponent>().LifeStyle.Transient);

		var factory = Container.Resolve<INonDisposableFactory>();

		var tracker = ReferenceTracker.Track(() => factory.Create());

		tracker.AssertStillReferencedAndDo(component => Container.Release(component));

		tracker.AssertStillReferenced();
	}

	[Fact]
	public void Component_released_via_disposing_factory_is_not_tracked()
	{
		Container.Register(
			Component.For<IDisposableFactory>().LifeStyle.Transient.AsFactory(),
			Component.For<DisposableComponent>().LifeStyle.Transient);

		var factory = Container.Resolve<IDisposableFactory>();

		var tracker = ReferenceTracker.Track(() => factory.Create());

		factory.Dispose();

		tracker.AssertNoLongerReferenced();
	}

	[Fact]
	public void Component_released_via_factory_is_not_tracked()
	{
		Container.Register(
			Component.For<INonDisposableFactory>().LifeStyle.Transient.AsFactory(),
			Component.For<DisposableComponent>().LifeStyle.Transient);

		var factory = Container.Resolve<INonDisposableFactory>();

		var tracker = ReferenceTracker.Track(() => factory.Create());

		tracker.AssertStillReferencedAndDo(component => factory.LetGo(component));

		tracker.AssertNoLongerReferenced();
	}

	[Fact]
	public void Disposing_factory_destroys_transient_components()
	{
		Container.Register(
			Component.For<IDisposableFactory>().AsFactory(),
			Component.For<DisposableComponent>().LifeStyle.Transient);
		var factory = Container.Resolve<IDisposableFactory>();
		var component = factory.Create();
		Assert.False(component.Disposed);

		factory.Dispose();
		Assert.True(component.Disposed);
	}

	[Fact]
	public void Disposing_factory_does_not_destroy_singleton_components()
	{
		Container.Register(
			Component.For<IDisposableFactory>().AsFactory(),
			Component.For<DisposableComponent>().LifeStyle.Singleton);
		var factory = Container.Resolve<IDisposableFactory>();
		var component = factory.Create();
		Assert.False(component.Disposed);

		factory.Dispose();
		Assert.False(component.Disposed);
	}

	[Fact]
	public void Disposing_factory_twice_does_not_throw()
	{
		Container.Register(
			Component.For<IDisposableFactory>().AsFactory());
		var factory = Container.Resolve<IDisposableFactory>();
		factory.Dispose();
		factory.Dispose();
	}

	[Fact]
	public void Release_product_after_disposing_factory_does_not_throw()
	{
		Container.Register(
			Component.For<IDisposableFactory>().AsFactory(),
			Component.For<DisposableComponent>().LifeStyle.Transient);
		var factory = Container.Resolve<IDisposableFactory>();
		var component = factory.Create();
		Assert.False(component.Disposed);

		factory.Dispose();
		Assert.True(component.Disposed);
		factory.Destroy(component);
	}

	[Fact]
	public void Factory_interface_can_be_hierarchical()
	{
		Container.Register(
			Component.For<ComponentWithOptionalParameter>()
				.LifeStyle.Transient,
			Component.For<IFactoryWithParametersExtended>()
				.AsFactory());
		var factory = Container.Resolve<IFactoryWithParametersExtended>();

		var one = factory.BuildComponent("one");
		var two = factory.BuildComponent2("two");
		Assert.Equal("one", one.Parameter);
		Assert.Equal("two", two.Parameter);
	}

	[Fact]
	public void Factory_interface_can_be_hierarchical_with_repetitions()
	{
		Container.Register(
			Component.For<ComponentWithOptionalParameter>()
				.LifeStyle.Transient,
			Component.For<IFactoryWithParametersTwoBases>()
				.AsFactory());
		var factory = Container.Resolve<IFactoryWithParametersTwoBases>();

		var one = factory.BuildComponent("one");
		var two = factory.BuildComponent2("two");
		var three = factory.BuildComponent2("three");
		Assert.Equal("one", one.Parameter);
		Assert.Equal("two", two.Parameter);
		Assert.Equal("three", three.Parameter);
	}

	[Fact]
	public void Factory_is_tracked_by_the_container()
	{
		Container.Register(Component.For<IDummyComponentFactory>().AsFactory());

		ReferenceTracker
			.Track(() => Container.Resolve<IDummyComponentFactory>())
			.AssertStillReferenced();
	}

	[Fact]
	public void Get_method_resolves_by_type_is_told_to_ignore_name()
	{
		Container.Register(
			Component.For<IDummyComponent>()
				.ImplementedBy<Component2>()
				.Named("SecondComponent")
				.LifeStyle.Transient,
			Component.For<IDummyComponentFactory>()
				.AsFactory(new DefaultTypedFactoryComponentSelector(false)));
		var factory = Container.Resolve<IDummyComponentFactory>();

		var component = factory.GetSecondComponent();

		Assert.NotNull(component);

		Assert.IsType<Component1>(component);
	}

	[Fact]
	public void Releasing_factory_release_components()
	{
		Container.Register(
			Component.For<INonDisposableFactory>().LifeStyle.Transient.AsFactory(),
			Component.For<DisposableComponent>().LifeStyle.Transient);
		var factory = Container.Resolve<INonDisposableFactory>();
		var component = factory.Create();
		Assert.False(component.Disposed);

		Container.Release(factory);
		Assert.True(component.Disposed);
	}

	[Fact]
	public void Releasing_factory_releases_selector()
	{
		DisposableSelector.InstancesCreated = 0;
		DisposableSelector.InstancesDisposed = 0;
		Container.Register(
			Component.For<IDummyComponentFactory>().AsFactory(f => f.SelectedWith<DisposableSelector>())
				.LifestyleTransient(),
			Component.For<DisposableSelector>().LifeStyle.Transient);
		var factory = Container.Resolve<IDummyComponentFactory>();

		Container.Release(factory);

		Assert.Equal(1, DisposableSelector.InstancesDisposed);
	}

	[Fact]
	public void Resolve_component_by_name_with_default_selector_fails_when_no_name_found()
	{
		Container.Register(
			Component.For<IDummyComponentFactory>()
				.AsFactory());
		var factory = Container.Resolve<IDummyComponentFactory>();

		Assert.Throws<ComponentNotFoundException>(() => factory.GetSecondComponent());
	}

	[Fact]
	public void Resolve_component_by_name_with_default_selector_falls_back_to_by_type_when_no_name_found_if_told_to()
	{
		Container.Register(
			Component.For<IDummyComponentFactory>()
				.AsFactory(new DefaultTypedFactoryComponentSelector(fallbackToResolveByTypeIfNameNotFound: true)));
		var factory = Container.Resolve<IDummyComponentFactory>();

		var component = factory.GetSecondComponent();
		Assert.NotNull(component);
		Assert.IsType<Component1>(component);
	}

	[Fact]
	public void Resolve_multiple_components_at_once_with_default_selector_collection_unasignable_from_array()
	{
		Container.Register(
			Component.For<IDummyComponent>()
				.ImplementedBy<Component2>()
				.LifeStyle.Transient,
			Component.For<INvalidDummyComponentListFactory>()
				.AsFactory());
		var factory = Container.Resolve<INvalidDummyComponentListFactory>();

		Assert.Throws<ComponentNotFoundException>(() => factory.All());
	}

	[Fact]
	public void Resolve_should_fail_hard_when_component_with_picked_name_not_present()
	{
		Container.Register(
			Component.For<IDummyComponent>().ImplementedBy<Component2>().LifestyleTransient(),
			Component.For<IDummyComponentFactory>().AsFactory(f => f.SelectedWith<FooSelector>()),
			Component.For<FooSelector>());
		var factory = Container.Resolve<IDummyComponentFactory>();

		Assert.Throws<ComponentNotFoundException>(() => factory.CreateDummyComponent());
	}

	[Fact]
	public void Selector_WILL_NOT_be_picked_implicitly()
	{
		Container.Register(
			Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
			Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
			Component.For<IDummyComponentFactory>().AsFactory(),
			Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>(),
			Component.For<Component2Selector, ITypedFactoryComponentSelector>());

		var factory = Container.Resolve<IDummyComponentFactory>();
		var component = factory.CreateDummyComponent();

		Assert.IsType<Component1>(component);
	}

	[Fact]
	public void Selector_pick_by_instance()
	{
		Container.Register(
			Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
			Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
			Component.For<IDummyComponentFactory>().AsFactory(c => c.SelectedWith(new Component2Selector())));

		var factory = Container.Resolve<IDummyComponentFactory>();
		var component = factory.CreateDummyComponent();

		Assert.IsType<Component2>(component);
	}

	[Fact]
	public void Selector_pick_by_name()
	{
		Container.Register(
			Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
			Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
			Component.For<IDummyComponentFactory>().AsFactory(c => c.SelectedWith("factoryTwo")),
			Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>().Named("factoryOne"),
			Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component2Selector>().Named("factoryTwo"));

		var factory = Container.Resolve<IDummyComponentFactory>();
		var component = factory.CreateDummyComponent();

		Assert.IsType<Component2>(component);
	}

	[Fact]
	public void Selector_pick_by_name_multiple_factories()
	{
		Container.Register(
			Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
			Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
			Component.For<IDummyComponentFactory>().AsFactory(c => c.SelectedWith("factoryTwo")).Named("2"),
			Component.For<IDummyComponentFactory>().AsFactory(c => c.SelectedWith("factoryOne")).Named("1"),
			Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>().Named("factoryOne"),
			Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component2Selector>().Named("factoryTwo"));

		var factory2 = Container.Resolve<IDummyComponentFactory>("2");
		var component2 = factory2.CreateDummyComponent();
		Assert.IsType<Component2>(component2);

		var factory1 = Container.Resolve<IDummyComponentFactory>("1");
		var component1 = factory1.CreateDummyComponent();
		Assert.IsType<Component1>(component1);
	}

	[Fact]
	public void Selector_pick_by_type()
	{
		Container.Register(
			Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
			Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
			Component.For<IDummyComponentFactory>().AsFactory(c => c.SelectedWith<Component2Selector>()),
			Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>(),
			Component.For<Component2Selector, ITypedFactoryComponentSelector>());

		var factory = Container.Resolve<IDummyComponentFactory>();
		var component = factory.CreateDummyComponent();

		Assert.IsType<Component2>(component);
	}

	[Fact]
	public void Selector_via_attribute_has_lower_priority_than_explicit_One()
	{
		Container.Register(
			Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
			Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
			Component.For<IDummyComponentFactoryWithAttributeImplementingType>()
				.AsFactory(c => c.SelectedWith<Component1Selector>()),
			Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>());

		var factory = Container.Resolve<IDummyComponentFactoryWithAttributeImplementingType>();
		var component = factory.CreateDummyComponent();

		Assert.IsType<Component1>(component);
	}

	[Fact]
	public void Selector_via_attribute_implementing_type()
	{
		Container.Register(
			Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
			Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
			Component.For<IDummyComponentFactoryWithAttributeImplementingType>().AsFactory());

		var factory = Container.Resolve<IDummyComponentFactoryWithAttributeImplementingType>();
		var component = factory.CreateDummyComponent();

		Assert.IsType<Component2>(component);
	}

	[Fact]
	public void Selector_via_attribute_service_name()
	{
		Container.Register(
			Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
			Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
			Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component2Selector>().Named("selector"),
			Component.For<IDummyComponentFactoryWithAttributeServiceName>().AsFactory());

		var factory = Container.Resolve<IDummyComponentFactoryWithAttributeServiceName>();
		var component = factory.CreateDummyComponent();

		Assert.IsType<Component2>(component);
	}

	[Fact]
	public void Selector_via_attribute_service_type()
	{
		Container.Register(
			Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
			Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
			Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component2Selector>(),
			Component.For<IDummyComponentFactoryWithAttributeServiceType>().AsFactory());

		var factory = Container.Resolve<IDummyComponentFactoryWithAttributeServiceType>();
		var component = factory.CreateDummyComponent();

		Assert.IsType<Component2>(component);
	}

	[Fact]
	public void Should_match_arguments_ignoring_case()
	{
		Container.Register(
			Component.For<IFactoryWithParameters>().AsFactory(),
			Component.For<ComponentWithOptionalParameter>());

		var factory = Container.Resolve<IFactoryWithParameters>();
		var component = factory.BuildComponent("foo");

		Assert.Equal("foo", component.Parameter);
	}

	[Fact]
	public void Typed_factory_lets_go_of_component_reference_on_dispose()
	{
		Container.Register(
			Component.For<IDisposableFactory>().LifeStyle.Transient.AsFactory(),
			Component.For<DisposableComponent>().LifeStyle.Transient);
		var factory = Container.Resolve<IDisposableFactory>();

		var tracker = ReferenceTracker.Track(() => factory.Create());

		factory.Dispose();

		tracker.AssertNoLongerReferenced();
	}

	[Fact]
	public void Typed_factory_lets_go_of_component_reference_on_release()
	{
		Container.Register(
			Component.For<IDisposableFactory>().LifeStyle.Transient.AsFactory(),
			Component.For<DisposableComponent>().LifeStyle.Transient);
		var factory = Container.Resolve<IDisposableFactory>();

		var tracker = ReferenceTracker.Track(() => factory.Create());

		tracker.AssertStillReferencedAndDo(component => factory.Destroy(component));

		tracker.AssertNoLongerReferenced();
	}

	[Fact]
	public void Typed_factory_obeys_release_policy_non_tracking()
	{
#pragma warning disable 612,618
		Container.Kernel.ReleasePolicy = new NoTrackingReleasePolicy();
#pragma warning restore 612,618
		Container.Register(
			Component.For<INonDisposableFactory>().LifeStyle.Transient.AsFactory(),
			Component.For<DisposableComponent>().LifeStyle.Transient);

		var factory = Container.Resolve<INonDisposableFactory>();

		ReferenceTracker
			.Track(() =>
			{
				var component = factory.Create();
				Container.Release(component);
				return component;
			})
			.AssertNoLongerReferenced();
	}

	[Fact]
	public void Typed_factory_obeys_release_policy_tracking()
	{
		Container.Register(
			Component.For<INonDisposableFactory>().LifeStyle.Transient.AsFactory(),
			Component.For<DisposableComponent>().LifeStyle.Transient);
		var factory = Container.Resolve<INonDisposableFactory>();

		ReferenceTracker
			.Track(() => factory.Create())
			.AssertStillReferenced();
	}

	[Fact]
	public void Void_methods_release_components()
	{
		Container.Register(
			Component.For<IDisposableFactory>().AsFactory(),
			Component.For<DisposableComponent>().LifeStyle.Transient);
		var factory = Container.Resolve<IDisposableFactory>();
		var component = factory.Create();
		Assert.False(component.Disposed);

		factory.Destroy(component);
		Assert.True(component.Disposed);
	}
}