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

using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.SubSystems.Conversion;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

public class TypedParametersTestCase : AbstractContainerTestCase
{
    [Fact]
    public void Can_mix_typed_arguments_with_named()
    {
        Kernel.Register(Component.For<ClassWithArguments>());
        var arguments = new Arguments
        {
            { "arg1", "foo" },
            { typeof(int), 2 }
        };

        var item = Kernel.Resolve<ClassWithArguments>(arguments);

        Assert.Equal("foo", item.Arg1);
        Assert.Equal(2, item.Arg2);
    }

    [Fact]
    public void Can_named_arguments_take_precedense_over_typed()
    {
        Kernel.Register(Component.For<ClassWithArguments>());
        var arguments = new Arguments
        {
            { "arg1", "named" },
            { typeof(string), "typed" },
            { typeof(int), 2 }
        };

        var item = Kernel.Resolve<ClassWithArguments>(arguments);

        Assert.Equal("named", item.Arg1);
        Assert.Equal(2, item.Arg2);
    }

    [Fact]
    public void Can_resolve_component_with_typed_arguments()
    {
        Kernel.Register(Component.For<ClassWithArguments>());
        var arguments = new Arguments
        {
            { typeof(string), "foo" },
            { typeof(int), 2 }
        };

        var item = Kernel.Resolve<ClassWithArguments>(arguments);

        Assert.Equal("foo", item.Arg1);
        Assert.Equal(2, item.Arg2);
    }

    [Fact]
    public void Typed_arguments_work_for_DynamicParameters()
    {
        Kernel.Register(
            Component.For<ClassWithArguments>().DynamicParameters((_, d) => d.AddTyped("typed").AddTyped(2)));

        var item = Kernel.Resolve<ClassWithArguments>();

        Assert.Equal("typed", item.Arg1);
        Assert.Equal(2, item.Arg2);
    }

    [Fact]
    public void Typed_arguments_work_for_DynamicParameters_mixed()
    {
        Kernel.Register(Component.For<ClassWithArguments>().DynamicParameters((_, d) => d.AddTyped("typed")));
        var arguments = new Arguments
        {
            { typeof(int), 2 }
        };
        var item = Kernel.Resolve<ClassWithArguments>(arguments);

        Assert.Equal("typed", item.Arg1);
        Assert.Equal(2, item.Arg2);
    }

    [Fact]
    public void Typed_arguments_work_for_DynamicParameters_mixed2()
    {
        Kernel.Register(Component.For<ClassWithArguments>());
        Assert.Throws<ConverterException>(() =>
        {
            Kernel.Resolve<ClassWithArguments>(new Arguments
            {
                { typeof(int), "not an int" },
                { typeof(string), "a string" }
            });
        });
    }

    [Fact]
    public void Typed_arguments_work_for_InLine_Parameters()
    {
        Kernel.Register(Component.For<ClassWithArguments>()
            .DependsOn(
                Property.ForKey<string>().Eq("typed"),
                Property.ForKey<int>().Eq(2)));

        var item = Kernel.Resolve<ClassWithArguments>();

        Assert.Equal("typed", item.Arg1);
        Assert.Equal(2, item.Arg2);
    }

    [Fact]
    public void Typed_arguments_work_for_ServiceOverrides()
    {
        Kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>().Named("default"));
        Kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl2>().Named("non-default"));
        Kernel.Register(Component.For<CommonServiceUser>()
            .DependsOn(ServiceOverride.ForKey<ICommon>().Eq("non-default")));

        var item = Kernel.Resolve<CommonServiceUser>();

        Assert.IsType<CommonImpl2>(item.CommonService);
    }

    [Fact]
    public void Typed_arguments_work_for_closed_generic_ServiceOverrides()
    {
        Kernel.Register(
            Component.For<IGeneric<string>>().ImplementedBy<GenericImpl1<string>>().Named("default"),
            Component.For<IGeneric<string>>().ImplementedBy<GenericImpl2<string>>().Named("non-default"),
            Component.For<UsesIGeneric<string>>()
                .DependsOn(ServiceOverride.ForKey<IGeneric<string>>().Eq("non-default")));

        var item = Kernel.Resolve<UsesIGeneric<string>>();

        Assert.IsType<GenericImpl2<string>>(item.Dependency);
    }

    [Fact]
    public void Typed_arguments_work_for_open_generic_ServiceOverrides_closed_service()
    {
        Kernel.Register(Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl1<>)).Named("default"));
        Kernel.Register(Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl2<>)).Named("non-default"));
        Kernel.Register(Component.For(typeof(UsesIGeneric<>))
            .DependsOn(ServiceOverride.ForKey<IGeneric<string>>().Eq("non-default")));

        var item = Kernel.Resolve<UsesIGeneric<string>>();

        Assert.IsType<GenericImpl2<string>>(item.Dependency);
    }

    [Fact]
    public void Typed_arguments_work_for_open_generic_ServiceOverrides_closed_service_preferred_over_open_service()
    {
        Kernel.Register(
            Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl1<>)).Named("default"),
            Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl2<>)).Named("non-default-open")
                .DependsOn(Property.ForKey("value").Eq(1)),
            Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl2<>)).Named("non-default-int")
                .DependsOn(Property.ForKey("value").Eq(2)),
            Component.For(typeof(UsesIGeneric<>))
                .DependsOn(ServiceOverride.ForKey(typeof(IGeneric<>)).Eq("non-default-open"),
                    ServiceOverride.ForKey<IGeneric<int>>().Eq("non-default-int"))
        );

        var withString = Kernel.Resolve<UsesIGeneric<string>>();
        var withInt = Kernel.Resolve<UsesIGeneric<int>>();

        Assert.IsType<GenericImpl2<string>>(withString.Dependency);
        Assert.Equal(1, ((GenericImpl2<string>)withString.Dependency).Value);
        Assert.IsType<GenericImpl2<int>>(withInt.Dependency);
        Assert.Equal(2, ((GenericImpl2<int>)withInt.Dependency).Value);
    }

    [Fact]
    public void Typed_arguments_work_for_open_generic_ServiceOverrides_open_service()
    {
        Kernel.Register(Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl1<>)).Named("default"));
        Kernel.Register(Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl2<>)).Named("non-default"));
        Kernel.Register(Component.For(typeof(UsesIGeneric<>))
            .DependsOn(ServiceOverride.ForKey(typeof(IGeneric<>)).Eq("non-default")));

        var item = Kernel.Resolve<UsesIGeneric<string>>();

        Assert.IsType<GenericImpl2<string>>(item.Dependency);
    }
}