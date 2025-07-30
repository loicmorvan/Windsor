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

using Castle.Windsor.MicroKernel.Handlers;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests.SpecializedResolvers;

public class ArrayResolverTestCase : AbstractContainerTestCase
{
    [Fact]
    public void ArrayResolution_UnresolvableDependencyCausesResolutionFailure()
    {
        Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel, true));
        Container.Register(
            Component.For<IDependency>().ImplementedBy<ResolvableDependency>(),
            Component.For<IDependency>().ImplementedBy<UnresolvableDependencyWithPrimitiveConstructor>(),
            Component.For<IDependOnArray>().ImplementedBy<DependsOnArray>()
        );

        var exception =
            Assert.Throws<HandlerException>(() => Container.Resolve<IDependOnArray>());

        var message =
            string.Format(
                "Can't create component '{1}' as it has dependencies to be satisfied.{0}{0}'{1}' is waiting for the following dependencies:{0}- Service '{2}' which was not registered.{0}- Parameter 'str' which was not provided. Did you forget to set the dependency?{0}",
                Environment.NewLine,
                typeof(UnresolvableDependencyWithPrimitiveConstructor).FullName,
                typeof(A).FullName);

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void ArrayResolution_UnresolvableDependencyCausesResolutionFailure_ServiceConstructor()
    {
        Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel, true));
        Container.Register(
            Component.For<IDependency>().ImplementedBy<ResolvableDependency>(),
            Component.For<IDependency>().ImplementedBy<UnresolvableDependencyWithAdditionalServiceConstructor>(),
            Component.For<IDependOnArray>().ImplementedBy<DependsOnArray>()
        );

        var exception =
            Assert.Throws<HandlerException>(() => Container.Resolve<IDependOnArray>());

        var message =
            string.Format(
                "Can't create component '{1}' as it has dependencies to be satisfied.{0}{0}'{1}' is waiting for the following dependencies:{0}- Service '{2}' which was not registered.{0}- Service '{3}' which was not registered.{0}",
                Environment.NewLine,
                typeof(UnresolvableDependencyWithAdditionalServiceConstructor).FullName,
                typeof(A).FullName,
                typeof(IEmptyService).FullName);

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void ArrayResolution_UnresolvableDependencyIsNotIncluded()
    {
        Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel));
        Container.Register(
            Component.For<IDependency>().ImplementedBy<ResolvableDependency>(),
            Component.For<IDependency>().ImplementedBy<UnresolvableDependency>(),
            Component.For<IDependOnArray>().ImplementedBy<DependsOnArray>()
        );

        var exception =
            Assert.Throws<HandlerException>(() => Container.Resolve<IDependOnArray>());

        var message =
            string.Format(
                "Can't create component '{1}' as it has dependencies to be satisfied.{0}{0}'{1}' is waiting for the following dependencies:{0}- Service '{2}' which was not registered.{0}",
                Environment.NewLine,
                typeof(UnresolvableDependency).FullName,
                typeof(A).FullName);

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Composite_service_can_be_resolved_without_triggering_circular_dependency_detection_fuse()
    {
        Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel));
        Container.Register(Classes.FromAssembly(GetCurrentAssembly())
            .BasedOn<IEmptyService>()
            .WithService.Base()
            .ConfigureFor<EmptyServiceComposite>(r => r.Forward<EmptyServiceComposite>()));

        var composite = Container.Resolve<EmptyServiceComposite>();
        Assert.Equal(5, composite.Inner.Length);
    }

    [Fact]
    public void
        Composite_service_can_be_resolved_without_triggering_circular_dependency_detection_fuse_composite_registered_first()
    {
        Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel));
        Container.Register(
            Component.For<IEmptyService, EmptyServiceComposite>().ImplementedBy<EmptyServiceComposite>(),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>(),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecoratorViaProperty>()
        );

        var composite = Container.Resolve<EmptyServiceComposite>();
        Assert.Equal(4, composite.Inner.Length);
    }

    [Fact]
    public void DependencyOnArrayOfServices_OnConstructor()
    {
        Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel));
        Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
            Component.For<ArrayDepAsConstructor>());

        var comp = Container.Resolve<ArrayDepAsConstructor>();

        Assert.NotNull(comp);
        Assert.NotNull(comp.Services);
        Assert.Equal(2, comp.Services.Length);
        foreach (var service in comp.Services)
        {
            Assert.NotNull(service);
        }
    }

    [Fact]
    public void DependencyOnArrayOfServices_OnProperty()
    {
        Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel));
        Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
            Component.For<ArrayDepAsProperty>());

        var comp = Container.Resolve<ArrayDepAsProperty>();

        Assert.NotNull(comp);
        Assert.NotNull(comp.Services);
        Assert.Equal(2, comp.Services.Length);
        foreach (var service in comp.Services)
        {
            Assert.NotNull(service);
        }
    }

    [Fact]
    public void DependencyOnArrayWhenEmpty()
    {
        Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel, true));
        Container.Register(Component.For<ArrayDepAsConstructor>(),
            Component.For<ArrayDepAsProperty>());

        var proxy = Container.Resolve<ArrayDepAsConstructor>();
        Assert.NotNull(proxy.Services);

        var proxy2 = Container.Resolve<ArrayDepAsProperty>();
        Assert.NotNull(proxy2.Services);
    }

    [Fact]
    public void DependencyOn_ref_ArrayOfServices_OnConstructor()
    {
        Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel));
        Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
            Component.For<ArrayRefDepAsConstructor>());

        var comp = Container.Resolve<ArrayRefDepAsConstructor>();

        Assert.NotNull(comp);
        Assert.NotNull(comp.Services);
        Assert.Equal(2, comp.Services.Length);
        foreach (var service in comp.Services)
        {
            Assert.NotNull(service);
        }
    }

    [Fact]
    public void InjectAll()
    {
        Container.Kernel.Resolver.AddSubResolver(new ArrayResolver(Container.Kernel, true));
        Container.Install(new CollectionServiceOverridesInstaller());
        var fooItemTest = Container.Resolve<ArrayDepAsConstructor>("InjectAll");
        var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
        Assert.Equal(3, dependencies.Count);
        Assert.Contains(typeof(EmptyServiceA), dependencies);
        Assert.Contains(typeof(EmptyServiceB), dependencies);
        Assert.Contains(typeof(EmptyServiceDecoratorViaProperty), dependencies);
    }

    [Fact]
    public void InjectFooAndBarOnly_WithArrayResolver()
    {
        Container.Kernel.Resolver.AddSubResolver(new ArrayResolver(Container.Kernel, true));
        Container.Install(new CollectionServiceOverridesInstaller());
        var fooItemTest = Container.Resolve<ArrayDepAsConstructor>("InjectFooAndBarOnly");
        var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
        Assert.Equal(2, dependencies.Count);
        Assert.Contains(typeof(EmptyServiceA), dependencies);
        Assert.Contains(typeof(EmptyServiceB), dependencies);
    }

    [Fact]
    public void InjectFooAndBarOnly_WithoutArrayResolver()
    {
        Container.Install(new CollectionServiceOverridesInstaller());
        var fooItemTest = Container.Resolve<ArrayDepAsConstructor>("InjectFooAndBarOnly");
        var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
        Assert.Equal(2, dependencies.Count);
        Assert.Contains(typeof(EmptyServiceA), dependencies);
        Assert.Contains(typeof(EmptyServiceB), dependencies);
    }

    [Fact]
    public void InjectFooOnly_WithArrayResolver()
    {
        Container.Kernel.Resolver.AddSubResolver(new ArrayResolver(Container.Kernel, true));
        Container.Install(new CollectionServiceOverridesInstaller());
        var fooItemTest = Container.Resolve<ArrayDepAsConstructor>("InjectFooOnly");
        var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
        Assert.Single(dependencies);
        Assert.Contains(typeof(EmptyServiceA), dependencies);
    }

    [Fact]
    public void InjectFooOnly_WithoutArrayResolver()
    {
        Container.Install(new CollectionServiceOverridesInstaller());
        var fooItemTest = Container.Resolve<ArrayDepAsConstructor>("InjectFooOnly");
        var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
        Assert.Single(dependencies);
        Assert.Contains(typeof(EmptyServiceA), dependencies);
    }
}