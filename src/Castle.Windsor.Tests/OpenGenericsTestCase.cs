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

using System.Collections.ObjectModel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

public class OpenGenericsTestCase : AbstractContainerTestCase
{
    [Fact]
    public void ExtendedProperties_incl_ProxyOptions_are_honored_for_open_generic_types()
    {
        Container.Register(
            Component.For(typeof(Collection<>))
                .Proxy.AdditionalInterfaces(typeof(ISimpleService)));

        var proxy = Container.Resolve<Collection<int>>();

        Assert.IsType<ISimpleService>(proxy, false);
    }

    [Fact]
    public void Open_generic_handlers_get_included_when_generic_service_requested()
    {
        Container.Register(Component.For<IGeneric<A>>().ImplementedBy<GenericImpl1<A>>(),
            Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl2<>)));

        var items = Container.ResolveAll<IGeneric<A>>();

        Assert.Equal(2, items.Length);
    }

    [Fact]
    public void Open_generic_multiple_services_favor_closed_service()
    {
        Container.Register(Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl1<>)),
            Component.For<A, IGeneric<A>>().ImplementedBy<GenericImplA>());

        var item = Container.Resolve<IGeneric<A>>();

        Assert.IsType<GenericImplA>(item);
    }

    [Fact]
    public void ResolveAll_properly_skips_open_generic_service_with_generic_constraints_that_dont_match()
    {
        Container.Register(
            Component.For(typeof(IHasGenericConstraints<,>))
                .ImplementedBy(typeof(HasGenericConstraintsImpl<,>)));

        var invalid = Container.ResolveAll<IHasGenericConstraints<EmptySub1, EmptyClass>>();

        Assert.Empty(invalid);
    }

    [Fact]
    public void ResolveAll_returns_matching_open_generic_service_with_generic_constraints()
    {
        Container.Register(
            Component.For(typeof(IHasGenericConstraints<,>))
                .ImplementedBy(typeof(HasGenericConstraintsImpl<,>)));

        var valid = Container.ResolveAll<IHasGenericConstraints<EmptySub2WithMarkerInterface, EmptyClass>>();

        Assert.Single(valid);
    }

    [Fact]
    public void Can_use_open_generic_with_LateBoundComponent_implementing_partial_closure()
    {
        Container.Register(
            Component.For(typeof(DoubleRepository<,>)).ImplementedBy(typeof(DoubleRepository<,>)),
            Component.For(typeof(ClassComponents.IRepository<>))
                .UsingFactoryMethod((k, c) =>
                {
                    var openType = typeof(DoubleRepository<,>);
                    var genericArgs = new[] { c.GenericArguments[0], typeof(int) };
                    var closedType = openType.MakeGenericType(genericArgs);
                    return k.Resolve(closedType);
                }));
        var repo = Container.Resolve<ClassComponents.IRepository<string>>();
        Assert.Null(repo.Find());
        Assert.IsType<DoubleRepository<string, int>>(repo);
    }
}