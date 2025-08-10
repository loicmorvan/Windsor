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

using System.Reflection;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

public class IsDefaultTestCase : AbstractContainerTestCase
{
    [Fact]
    public void Can_make_a_component_default_via_AllTypes_1()
    {
        Container.Register(
            Classes.FromAssembly(GetCurrentAssembly())
                .BasedOn<IEmptyService>()
                .WithService.Base()
                .ConfigureFor<EmptyServiceB>(c => c.IsDefault()));
        var obj = Container.Resolve<IEmptyService>();

        Assert.IsType<EmptyServiceB>(obj);
    }

    [Fact]
    public void Can_make_a_component_default_via_AllTypes_2()
    {
        Container.Register(
            Classes.FromAssembly(GetCurrentAssembly())
                .BasedOn<IEmptyService>()
                .WithService.Base()
                .ConfigureFor<EmptyServiceA>(c => c.IsDefault()));
        var obj = Container.Resolve<IEmptyService>();

        Assert.IsType<EmptyServiceA>(obj);
    }

    [Fact]
    public void Can_make_non_first_component_default()
    {
        Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>().IsDefault());

        var obj = Container.Resolve<IEmptyService>();

        Assert.IsType<EmptyServiceB>(obj);
    }

    [Fact]
    public void Can_make_non_first_component_default_with_filter()
    {
        Container.Register(Component.For<IEmptyService, EmptyServiceA, object>().ImplementedBy<EmptyServiceA>(),
            Component.For<IEmptyService, EmptyServiceB, object>().ImplementedBy<EmptyServiceB>()
                .IsDefault(t => t.GetTypeInfo().IsInterface));

        var obj = Container.Resolve<IEmptyService>();

        Assert.IsType<EmptyServiceB>(obj);

        var obj2 = Container.Resolve<object>();
        Assert.IsType<EmptyServiceA>(obj2);
    }

    [Fact]
    public void Does_affect_order_when_using_ResolveAll()
    {
        Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>().IsDefault(t => t.GetTypeInfo().IsInterface));

        var obj = Container.ResolveAll<IEmptyService>();

        Assert.IsType<EmptyServiceB>(obj[0]);
        Assert.IsType<EmptyServiceA>(obj[1]);
    }

    [Fact]
    public void Later_default_overrides_earlier_one()
    {
        Container.Register(
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>().IsDefault(t => t.GetTypeInfo().IsInterface),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>().IsDefault(t => t.GetTypeInfo().IsInterface));

        var obj = Container.Resolve<IEmptyService>();

        Assert.IsType<EmptyServiceB>(obj);
    }
}