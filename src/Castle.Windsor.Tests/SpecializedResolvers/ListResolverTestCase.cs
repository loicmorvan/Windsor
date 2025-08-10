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

using Castle.DynamicProxy;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Windsor;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Castle.Windsor.Tests.SpecializedResolvers;

public class ListResolverTestCase : AbstractContainerTestCase
{
    protected override WindsorContainer BuildContainer()
    {
        var container = new WindsorContainer();
        container.Kernel.Resolver.AddSubResolver(new ListResolver(container.Kernel));
        return container;
    }

    [Fact]
    public void DependencyOnListOfInterceptedServices()
    {
        Kernel.Register(
            Component.For<StandardInterceptor>().Named("a"),
            Component.For<StandardInterceptor>().Named("b"),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>().Interceptors("a"),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>().Interceptors("b"),
            Component.For<ListDepAsConstructor>(),
            Component.For<ListDepAsProperty>());

        var proxy = Kernel.Resolve<ListDepAsConstructor>().Services[0] as IProxyTargetAccessor;
        Assert.NotNull(proxy);
        Assert.Same(proxy.GetInterceptors()[0], Kernel.Resolve<StandardInterceptor>("a"));

        proxy = Kernel.Resolve<ListDepAsConstructor>().Services[1] as IProxyTargetAccessor;
        Assert.NotNull(proxy);
        Assert.Same(proxy.GetInterceptors()[0], Kernel.Resolve<StandardInterceptor>("b"));

        proxy = Kernel.Resolve<ListDepAsProperty>().Services[0] as IProxyTargetAccessor;
        Assert.NotNull(proxy);
        Assert.Same(proxy.GetInterceptors()[0], Kernel.Resolve<StandardInterceptor>("a"));

        proxy = Kernel.Resolve<ListDepAsProperty>().Services[1] as IProxyTargetAccessor;
        Assert.NotNull(proxy);
        Assert.Same(proxy.GetInterceptors()[0], Kernel.Resolve<StandardInterceptor>("b"));
    }

    [Fact]
    public void DependencyOnListOfServices_OnConstructor()
    {
        Kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
            Component.For<ListDepAsConstructor>());

        var comp = Kernel.Resolve<ListDepAsConstructor>();

        Assert.NotNull(comp);
        Assert.NotNull(comp.Services);
        Assert.Equal(2, comp.Services.Count);
        foreach (var service in comp.Services.AsEnumerable())
        {
            Assert.NotNull(service);
        }
    }

    [Fact]
    public void DependencyOnListOfServices_OnProperty()
    {
        Kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
            Component.For<ListDepAsProperty>());

        var comp = Kernel.Resolve<ListDepAsProperty>();

        Assert.NotNull(comp);
        Assert.NotNull(comp.Services);
        Assert.Equal(2, comp.Services.Count);
        foreach (var service in comp.Services.AsEnumerable())
        {
            Assert.NotNull(service);
        }
    }

    [Fact]
    public void DependencyOnListWhenEmpty()
    {
        Kernel.Resolver.AddSubResolver(new ListResolver(Kernel, true));
        Kernel.Register(Component.For<ListDepAsConstructor>(),
            Component.For<ListDepAsProperty>());

        var proxy = Kernel.Resolve<ListDepAsConstructor>();
        Assert.NotNull(proxy.Services);

        var proxy2 = Kernel.Resolve<ListDepAsProperty>();
        Assert.NotNull(proxy2.Services);
    }

    [Fact]
    public void Honors_collection_override_all_components_in()
    {
        Container.Install(new CollectionServiceOverridesInstaller());
        var fooItemTest = Container.Resolve<ListDepAsConstructor>("InjectAllList");
        var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
        Assert.Equal(3, dependencies.Count);
        Assert.Contains(typeof(EmptyServiceA), dependencies);
        Assert.Contains(typeof(EmptyServiceB), dependencies);
        Assert.Contains(typeof(EmptyServiceDecoratorViaProperty), dependencies);
    }

    [Fact]
    public void Honors_collection_override_one_components_in()
    {
        Container.Install(new CollectionServiceOverridesInstaller());
        var fooItemTest = Container.Resolve<ListDepAsConstructor>("InjectFooOnlyList");
        var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
        Assert.Single(dependencies);
        Assert.Contains(typeof(EmptyServiceA), dependencies);
    }

    [Fact]
    public void Honors_collection_override_one_components_in_no_resolver()
    {
        var container = new WindsorContainer();
        container.Install(new CollectionServiceOverridesInstaller());
        var fooItemTest = container.Resolve<ListDepAsConstructor>("InjectFooOnlyList");
        var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
        Assert.Single(dependencies);
        Assert.Contains(typeof(EmptyServiceA), dependencies);
    }

    [Fact]
    public void Honors_collection_override_some_components_in()
    {
        Container.Install(new CollectionServiceOverridesInstaller());
        var fooItemTest = Container.Resolve<ListDepAsConstructor>("InjectFooAndBarOnlyList");
        var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
        Assert.Equal(2, dependencies.Count);
        Assert.Contains(typeof(EmptyServiceA), dependencies);
        Assert.Contains(typeof(EmptyServiceB), dependencies);
    }

    [Fact]
    public void Honors_collection_override_some_components_in_no_resolver()
    {
        var container = new WindsorContainer();
        container.Install(new CollectionServiceOverridesInstaller());
        var fooItemTest = container.Resolve<ListDepAsConstructor>("InjectFooAndBarOnlyList");
        var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
        Assert.Equal(2, dependencies.Count);
        Assert.Contains(typeof(EmptyServiceA), dependencies);
        Assert.Contains(typeof(EmptyServiceB), dependencies);
    }
}