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

using System.Reflection;
using Castle.Windsor.Facilities.TypedFactory;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Releasers;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Facilities.TypedFactory.Components;
using Castle.Windsor.Tests.Facilities.TypedFactory.Delegates;
using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;
using Castle.Windsor.Tests.Facilities.TypedFactory.Selectors;
using Castle.Windsor.Tests.Interceptors;
using JetBrains.Annotations;
using HasTwoConstructors = Castle.Windsor.Tests.Facilities.TypedFactory.Delegates.HasTwoConstructors;
using ServiceFactory = Castle.Windsor.Tests.Facilities.TypedFactory.Components.ServiceFactory;

namespace Castle.Windsor.Tests.Facilities.TypedFactory;

using HasTwoConstructors = HasTwoConstructors;
using ServiceFactory = ServiceFactory;

public class TypedFactoryDelegatesTestCase : AbstractContainerTestCase
{
    protected override void AfterContainerCreated()
    {
        Container.AddFacility<TypedFactoryFacility>();
    }

    [Fact]
    public void Can_register_generic_delegate_factory_explicitly_as_open_generic_optional_dependency()
    {
        Container.Register(Component.For<Foo>().LifeStyle.Transient,
            Component.For<Bar>().LifeStyle.Transient,
            Component.For<UsesFooAndBarDelegateProperties>(),
            Component.For(typeof(Func<>)).AsFactory());

        var instance = Container.Resolve<UsesFooAndBarDelegateProperties>();

        Assert.NotNull(instance.FooFactory);
        Assert.NotNull(instance.BarFactory);

        var factoryHandler = Kernel.GetHandler(typeof(Func<>));
        Assert.NotNull(factoryHandler);

        var allhandlers = Kernel.GetAssignableHandlers(typeof(object));

        Assert.DoesNotContain(allhandlers.SelectMany(h => h.ComponentModel.Services), s => s == typeof(Func<Foo>));
        Assert.DoesNotContain(allhandlers.SelectMany(h => h.ComponentModel.Services), s => s == typeof(Func<Bar>));
    }

    [Fact]
    public void Can_register_generic_delegate_factory_explicitly_as_open_generic_required_dependency()
    {
        Container.Register(Component.For<Foo>().LifeStyle.Transient,
            Component.For<Bar>().LifeStyle.Transient,
            Component.For<UsesFooAndBarDelegateCtor>(),
            Component.For(typeof(Func<>)).AsFactory());

        var instance = Container.Resolve<UsesFooAndBarDelegateCtor>();

        Assert.NotNull(instance.FooFactory);
        Assert.NotNull(instance.BarFactory);

        var factoryHandler = Kernel.GetHandler(typeof(Func<>));
        Assert.NotNull(factoryHandler);

        var allhandlers = Kernel.GetAssignableHandlers(typeof(object));

        Assert.DoesNotContain(allhandlers.SelectMany(h => h.ComponentModel.Services), s => s == typeof(Func<Foo>));
        Assert.DoesNotContain(allhandlers.SelectMany(h => h.ComponentModel.Services), s => s == typeof(Func<Bar>));
    }

    [Fact]
    public void Can_resolve_component_depending_on_delegate_when_inline_argumens_are_provided()
    {
        Container.Register(Component.For<Foo>(),
            Component.For<UsesFooDelegateAndInt>());

        Container.Resolve<UsesFooDelegateAndInt>(Arguments.FromProperties(new { additionalArgument = 5 }));
    }

    [Fact]
    public void Can_resolve_delegate_of_generic()
    {
        Container.Register(Component.For(typeof(GenericComponent<>)).LifeStyle.Transient);
        var one = Container.Resolve<Func<GenericComponent<int>>>();
        var two = Container.Resolve<Func<GenericComponent<string>>>();
        one();
        two();
    }

    [Fact]
    public void Can_resolve_generic_component_depending_on_delegate_of_generic()
    {
        Container.Register(Component.For(typeof(GenericComponent<>)).LifeStyle.Transient,
            Component.For(typeof(GenericUsesFuncOfGenerics<>)).LifeStyle.Transient);
        var one = Container.Resolve<GenericUsesFuncOfGenerics<int>>();
        var two = Container.Resolve<GenericUsesFuncOfGenerics<string>>();
        one.Func();
        two.Func();
    }

    [Fact]
    public void Can_resolve_multiple_delegates()
    {
        Container.Register(Component.For<Baz>());
        Container.Register(Component.For<A>());

        var bazFactory = Container.Resolve<Func<Baz>>();
        var aFactory = Container.Resolve<Func<A>>();

        bazFactory.Invoke();
        aFactory.Invoke();
    }

    [Fact]
    public void Can_resolve_service_via_delegate()
    {
        Container.Register(Component.For<Foo>().Named("MyFoo").LifeStyle.Transient);
        Container.Register(Component.For<UsesFooDelegate>());
        var dependsOnFoo = Container.Resolve<UsesFooDelegate>();
        var foo = dependsOnFoo.GetFoo();
        Assert.Equal(1, foo.Number);
        foo = dependsOnFoo.GetFoo();
        Assert.Equal(2, foo.Number);
    }

    [Fact]
    public void Can_resolve_two_services_depending_on_identical_delegates()
    {
        Container.Register(Component.For<Foo>().LifeStyle.Transient,
            Component.For<UsesFooDelegate>(),
            Component.For<UsesFooDelegateAndInt>().DependsOn(new Arguments().AddTyped(5)));
        var one = Container.Resolve<UsesFooDelegate>();
        var two = Container.Resolve<UsesFooDelegateAndInt>();
        one.GetFoo();
        two.GetFoo();
    }

    [Fact]
    public void Can_resolve_two_services_depending_on_identical_delegates_via_interface_based_factory()
    {
        Container.Register(Component.For<Foo>().LifeStyle.Transient,
            Component.For<UsesFooDelegate>(),
            Component.For<UsesFooDelegateAndInt>().DependsOn(new Arguments().AddTyped(5)),
            Component.For<IGenericComponentsFactory>().AsFactory());

        var factory = Container.Resolve<IGenericComponentsFactory>();

        var one = factory.CreateGeneric<UsesFooDelegate>();
        var two = factory.CreateGeneric<UsesFooDelegateAndInt>();

        one.GetFoo();
        two.GetFoo();
    }

    [Fact]
    public void Can_use_additional_interceptors_on_delegate_based_factory()
    {
        Container.Register(
            Component.For<CollectInvocationsInterceptor>(),
            Component.For<IDummyComponent>().ImplementedBy<Component1>(),
            Component.For<Func<IDummyComponent>>().Interceptors<CollectInvocationsInterceptor>().AsFactory());
        var factory = Container.Resolve<Func<IDummyComponent>>();

        var component = factory();
        Assert.NotNull(component);

        var interceptor = Container.Resolve<CollectInvocationsInterceptor>();

        Assert.Single(interceptor.Invocations);
        Assert.Same(component, interceptor.Invocations[0].ReturnValue);
    }

    [Fact]
    public void Explicitly_registered_factory_is_tracked()
    {
        Container.Register(Component.For<Func<A>>().AsFactory());

        ReferenceTracker
            .Track(() => Container.Resolve<Func<A>>())
            .AssertStillReferenced();
    }

    [Fact]
    public void Factory_DOES_NOT_implicitly_pick_registered_selector_explicitly_registered_factory()
    {
        Container.Register(
            Component.For<ITypedFactoryComponentSelector>().ImplementedBy<NotInstantiableSelector>().LifeStyle
                .Transient,
            Component.For<Func<Foo>>().LifeStyle.Transient.AsFactory());

        Container.Resolve<Func<Foo>>();
    }

    [Fact]
    public void Factory_DOES_NOT_implicitly_pick_registered_selector_implicitly_registered_factory()
    {
        Container.Register(
            Component
                .For<ITypedFactoryComponentSelector>()
                .ImplementedBy<NotInstantiableSelector>()
                .LifeStyle.Transient);

        Container.Resolve<Func<Foo>>();
    }

    [Fact]
    public void Factory_affects_constructor_resolution()
    {
        Container.Register(Component.For<Baz>().Named("baz"));
        Container.Register(Component.For<HasTwoConstructors>().Named("fizz"));
        var factory = Container.Resolve<Func<string, HasTwoConstructors>>();

        var obj = factory("naaaameee");
        Assert.Equal("naaaameee", obj.Name);
    }

    [Fact]
    public void
        Factory_constructor_dependency_is_satisfied_implicitly_even_if_less_greedy_constructor_is_readily_available()
    {
        Container.Register(Component.For<Bar>().LifeStyle.Transient,
            Component.For<UsesBarDelegateTwoConstructors>().LifeStyle.Transient);

        var component = Container.Resolve<UsesBarDelegateTwoConstructors>();

        Assert.NotNull(component.BarFactory);
    }

    [Fact]
    public void Factory_does_not_reference_components_after_they_are_released()
    {
        var counter = new LifecycleCounter();

        Container.Register(
            Component
                .For<DisposableFoo>()
                .LifeStyle.Transient
                .DependsOn(Arguments.FromTyped([counter])),
            Component
                .For<UsesDisposableFooDelegate>()
                .LifeStyle.Transient);
        var dependsOnFoo = Container.Resolve<UsesDisposableFooDelegate>();

        var tracker = ReferenceTracker.Track(() => dependsOnFoo.GetFoo());

        Assert.Equal(0, counter["Dispose"]);
        Container.Release(dependsOnFoo);
        Assert.Equal(1, counter["Dispose"]);

        tracker.AssertNoLongerReferenced();
    }

    [Fact]
    public void Factory_explicitly_pick_registered_selector()
    {
        SimpleSelector.InstancesCreated = 0;
        Container.Register(
            Component.For<ITypedFactoryComponentSelector>().ImplementedBy<NotInstantiableSelector>().Named("1")
                .LifeStyle.Transient,
            Component.For<ITypedFactoryComponentSelector>().ImplementedBy<SimpleSelector>().Named("2").LifeStyle
                .Transient,
            Component.For<Func<Foo>>().LifeStyle.Transient.AsFactory(x => x.SelectedWith("2")));

        Container.Resolve<Func<Foo>>();

        Assert.Equal(1, SimpleSelector.InstancesCreated);
    }

    [Fact]
    public void Factory_obeys_lifestyle()
    {
        Container.Register(Component.For<Foo>().Named("MyFoo").LifeStyle.Singleton);
        Container.Register(Component.For<UsesFooDelegate>());
        var dependsOnFoo = Container.Resolve<UsesFooDelegate>();
        var foo = dependsOnFoo.GetFoo();
        Assert.Equal(1, foo.Number);
        foo = dependsOnFoo.GetFoo();
        Assert.Equal(1, foo.Number);
    }

    [Fact]
    public void Factory_obeys_release_policy_non_tracking()
    {
#pragma warning disable 612,618
        Container.Kernel.ReleasePolicy = new NoTrackingReleasePolicy();
#pragma warning restore 612,618
        Container.Register(
            Component.For<LifecycleCounter>(),
            Component.For<DisposableFoo>().LifeStyle.Transient,
            Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);
        var dependsOnFoo = Container.Resolve<UsesDisposableFooDelegate>();

        ReferenceTracker
            .Track(() => dependsOnFoo.GetFoo())
            .AssertNoLongerReferenced();
    }

    [Fact]
    public void Factory_obeys_release_policy_tracking()
    {
        Container.Register(
            Component.For<LifecycleCounter>(),
            Component.For<DisposableFoo>().LifeStyle.Transient,
            Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);

        var dependsOnFoo = Container.Resolve<UsesDisposableFooDelegate>();

        ReferenceTracker
            .Track(() => dependsOnFoo.GetFoo())
            .AssertStillReferenced();
    }

    [Fact]
    public void Factory_property_dependency_is_satisfied_implicitly()
    {
        Container.Register(Component.For<Bar>().LifeStyle.Transient,
            Component.For<UsesBarDelegateProperty>().LifeStyle.Transient);

        var component = Container.Resolve<UsesBarDelegateProperty>();

        Assert.NotNull(component.BarFactory);
    }

    [Fact]
    public void Func_delegate_with_duplicated_Parameter_types_throws_exception()
    {
        Container.Register(Component.For<Baz>().Named("baz"),
            Component.For<Bar>().Named("bar"),
            Component.For<UsesBarDelegate>());

        var user = Container.Resolve<UsesBarDelegate>();

        var exception =
            Assert.Throws<ArgumentException>(() =>
                user.GetBar("aaa", "bbb"));

        Assert.Equal(
            "Factory delegate System.Func`3[System.String,System.String,Castle.Windsor.Tests.Facilities.TypedFactory.Delegates.Bar] has duplicated arguments of type System.String. " +
            "Using generic purpose delegates with duplicated argument types is unsupported, because then it is not possible to match arguments properly. " +
            "Use some custom delegate with meaningful argument names or interface based factory instead.",
            exception.Message);
    }

    [Fact]
    public void Implicitly_registered_factory_is_always_tracked()
    {
        var factory = Container.Resolve<Func<A>>();

        Assert.True(Container.Kernel.ReleasePolicy.HasTrack(factory));
    }

    [Fact]
    public void Registered_Delegate_prefered_over_factory()
    {
        var foo = new DisposableFoo(new LifecycleCounter());
        Container.Register(Component.For<DisposableFoo>().LifeStyle.Transient,
            Component.For<Func<int, DisposableFoo>>().Instance(_ => foo),
            Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);
        var dependsOnFoo = Container.Resolve<UsesDisposableFooDelegate>();
        var otherFoo = dependsOnFoo.GetFoo();
        Assert.Same(foo, otherFoo);
    }

    [Fact]
    public void Registers_generic_delegate_factories_as_open_generics_optional_dependency()
    {
        Container.Register(Component.For<Foo>().LifeStyle.Transient,
            Component.For<Bar>().LifeStyle.Transient,
            Component.For<UsesFooAndBarDelegateProperties>());

        var instance = Container.Resolve<UsesFooAndBarDelegateProperties>();

        Assert.NotNull(instance.FooFactory);
        Assert.NotNull(instance.BarFactory);

        var factoryHandler = Kernel.GetHandler(typeof(Func<>));
        Assert.NotNull(factoryHandler);
    }

    [Fact]
    public void Registers_generic_delegate_factories_as_open_generics_required_dependency()
    {
        Container.Register(Component.For<Foo>().LifeStyle.Transient,
            Component.For<Bar>().LifeStyle.Transient,
            Component.For<UsesFooAndBarDelegateCtor>());

        var instance = Container.Resolve<UsesFooAndBarDelegateCtor>();

        Assert.NotNull(instance.FooFactory);
        Assert.NotNull(instance.BarFactory);

        var factoryHandler = Kernel.GetHandler(typeof(Func<>));
        Assert.NotNull(factoryHandler);
    }

    [Fact]
    public void Releasing_component_depending_on_a_factory_releases_what_was_pulled_from_it()
    {
        var counter = new LifecycleCounter();

        Container.Register(
            Component
                .For<DisposableFoo>()
                .LifeStyle.Transient
                .DependsOn(Arguments.FromTyped([counter])),
            Component
                .For<UsesDisposableFooDelegate>()
                .LifeStyle.Transient);
        var dependsOnFoo = Container.Resolve<UsesDisposableFooDelegate>();
        dependsOnFoo.GetFoo();

        Assert.Equal(0, counter["Dispose"]);
        Container.Release(dependsOnFoo);
        Assert.Equal(1, counter["Dispose"]);
    }

    [Fact]
    public void Releasing_factory_releases_selector()
    {
        var counter = new LifecycleCounter();

        Container.Register(
            Component.For<LifecycleCounter>().Instance(counter),
            Component.For<SelectorWithLifecycleCounter>().LifeStyle.Transient,
            Component.For<Func<Foo>>().LifeStyle.Transient
                .AsFactory(x => x.SelectedWith<SelectorWithLifecycleCounter>()));
        var factory = Container.Resolve<Func<Foo>>();

        Assert.Equal(1, counter[".ctor"]);

        Container.Release(factory);

        Assert.Equal(1, counter["Dispose"]);
    }

    [Fact]
    public void
        Resolution_ShouldNotThrow_When_TwoDelegateFactoriesAreResolvedWithOnePreviouslyLazyLoaded_WithMultipleCtors()
    {
        Container.Register(Component.For<SimpleComponent1>(),
            Component.For<SimpleComponent2>(),
            Component.For<SimpleComponent3>(),
            Component.For<ServiceFactory>(),
            Component.For<ServiceRedirect>(),
            Component.For<ServiceWithMultipleCtors>());

        var factory = Container.Resolve<ServiceFactory>();
        factory.Factory();
    }

    [Fact]
    public void Selector_pick_by_name()
    {
        Container.Register(
            Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
            Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
            Component.For<Func<IDummyComponent>>().AsFactory(c => c.SelectedWith("factoryTwo")),
            Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>().Named("factoryOne"),
            Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component2Selector>().Named("factoryTwo"));

        Assert.True(Container.Kernel.HasComponent(typeof(Func<IDummyComponent>)));
        var factory = Container.Resolve<Func<IDummyComponent>>();
        var component = factory.Invoke();

        Assert.IsType<Component2>(component);
    }

    public class NotInstantiableSelector : ITypedFactoryComponentSelector
    {
        public NotInstantiableSelector()
        {
            Assert.Fail();
        }

        public Func<IKernelInternal, IReleasePolicy, object> SelectComponent(MethodInfo method,
            object[] arguments)
        {
            throw new NotImplementedException();
        }
    }

    [UsedImplicitly]
    public sealed class SelectorWithLifecycleCounter : ITypedFactoryComponentSelector, IDisposable
    {
        private readonly LifecycleCounter _counter;

        public SelectorWithLifecycleCounter(LifecycleCounter counter)
        {
            _counter = counter;
            counter.Increment();
        }

        public void Dispose()
        {
            _counter.Increment();
        }

        public Func<IKernelInternal, IReleasePolicy, object> SelectComponent(MethodInfo method,
            object[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}