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

using Castle.Core.Configuration;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.ComponentActivator;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.SubSystems.Configuration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests.Activators;

[RelatedTestCase(typeof(HelpfulExceptionsOnResolveTestCase),
    "Some tests about exceptions thrown when constructor not available.")]
public class BestConstructorTestCase : AbstractContainerTestCase
{
    [Fact]
    public void ConstructorWithMoreArguments()
    {
        Container.Register(Component.For<A>(),
            Component.For<B>(),
            Component.For<C>(),
            Component.For<ServiceUser>());

        var service = Container.Resolve<ServiceUser>();

        Assert.NotNull(service);
        Assert.NotNull(service.AComponent);
        Assert.NotNull(service.BComponent);
        Assert.NotNull(service.CComponent);
    }

    [Fact]
    public void ConstructorWithOneArgument()
    {
        Container.Register(Component.For<A>().Named("a"),
            Component.For<ServiceUser>().Named("service"));

        var service = Container.Resolve<ServiceUser>("service");

        Assert.NotNull(service);
        Assert.NotNull(service.AComponent);
        Assert.Null(service.BComponent);
        Assert.Null(service.CComponent);
    }

    [Fact]
    public void ConstructorWithTwoArguments()
    {
        Container.Register(Component.For<A>().Named("a"),
            Component.For<B>().Named("b"),
            Component.For<ServiceUser>().Named("service"));

        var service = Container.Resolve<ServiceUser>("service");

        Assert.NotNull(service);
        Assert.NotNull(service.AComponent);
        Assert.NotNull(service.BComponent);
        Assert.Null(service.CComponent);
    }

    [Fact]
    public void DefaultComponentActivator_is_used_by_default()
    {
        Container.Register(Component.For<A>());

        var handler = Kernel.GetHandler(typeof(A));
        var activator = ((IKernelInternal)Kernel).CreateComponentActivator(handler.ComponentModel);

        Assert.IsType<DefaultComponentActivator>(activator);
    }

    [Fact]
    public void ParametersAndServicesBestCase()
    {
        var store = new DefaultConfigurationStore();

        var config = new MutableConfiguration("component");
        var parameters = new MutableConfiguration("parameters");
        config.Children.Add(parameters);
        parameters.Children.Add(new MutableConfiguration("name", "hammett"));
        parameters.Children.Add(new MutableConfiguration("port", "120"));

        store.AddComponentConfiguration("service", config);

        Kernel.ConfigurationStore = store;

        Container.Register(Component.For<A>().Named("a"),
            Component.For<ServiceUser2>().Named("service"));

        var service = Container.Resolve<ServiceUser2>("service");

        Assert.NotNull(service);
        Assert.NotNull(service.AComponent);
        Assert.Null(service.BComponent);
        Assert.Null(service.CComponent);
        Assert.Equal("hammett", service.Name);
        Assert.Equal(120, service.Port);
    }

    [Fact]
    public void ParametersAndServicesBestCase2()
    {
        var store = new DefaultConfigurationStore();

        var config = new MutableConfiguration("component");
        var parameters = new MutableConfiguration("parameters");
        config.Children.Add(parameters);
        parameters.Children.Add(new MutableConfiguration("name", "hammett"));
        parameters.Children.Add(new MutableConfiguration("port", "120"));
        parameters.Children.Add(new MutableConfiguration("Scheduleinterval", "22"));

        store.AddComponentConfiguration("service", config);

        Kernel.ConfigurationStore = store;

        Container.Register(Component.For<A>().Named("a"),
            Component.For<ServiceUser2>().Named("service"));

        var service = Container.Resolve<ServiceUser2>("service");

        Assert.NotNull(service);
        Assert.NotNull(service.AComponent);
        Assert.Null(service.BComponent);
        Assert.Null(service.CComponent);
        Assert.Equal("hammett", service.Name);
        Assert.Equal(120, service.Port);
        Assert.Equal(22, service.ScheduleInterval);
    }

    [Fact]
    public void Two_constructors_but_one_with_satisfiable_dependencies()
    {
        Container.Register(Component.For<SimpleComponent1>(),
            Component.For<SimpleComponent2>(),
            Component.For<HasTwoConstructors3>());
        var component = Container.Resolve<HasTwoConstructors3>();
        Assert.NotNull(component.X);
        Assert.NotNull(component.Y);
        Assert.Null(component.A);
    }

    [Fact]
    public void Two_constructors_but_one_with_satisfiable_dependencies_issue_IoC_209()
    {
        Container.Register(Component.For<SimpleComponent1>(),
            Component.For<SimpleComponent2>(),
            Component.For<HasTwoConstructors4>());

        Container.Resolve<HasTwoConstructors4>();
    }

    [Fact]
    public void Two_constructors_but_one_with_satisfiable_dependencies_registering_dependencies_last()
    {
        Container.Register(Component.For<HasTwoConstructors3>(),
            Component.For<SimpleComponent1>(),
            Component.For<SimpleComponent2>());
        var component = Container.Resolve<HasTwoConstructors3>();
        Assert.NotNull(component.X);
        Assert.NotNull(component.Y);
        Assert.Null(component.A);
    }

    [Fact]
    public void Two_constructors_equal_number_of_parameters_pick_one_that_can_be_satisfied()
    {
        Container.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
            Component.For<HasTwoConstructors>());

        Container.Resolve<HasTwoConstructors>();
    }

    [Fact]
    public void Two_satisfiable_constructors_identical_dependency_kinds_pick_based_on_parameter_names()
    {
        Container.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
            Component.For<ICustomer>().ImplementedBy<CustomerImpl>(),
            Component.For<HasTwoConstructors>().Properties(PropertyFilter.IgnoreAll));

        var component = Container.Resolve<HasTwoConstructors>();

        // common is 'smaller' so we pick ctor with dependency named 'common'
        Assert.NotNull(component.Common);
    }

    [Fact]
    public void Two_satisfiable_constructors_pick_one_with_more_inline_parameters()
    {
        Container.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
            Component.For<HasTwoConstructors2>()
                .DependsOn(Parameter.ForKey("param").Eq("foo")));

        var component = Container.Resolve<HasTwoConstructors2>();

        Assert.Equal("foo", component.Param);
    }
}