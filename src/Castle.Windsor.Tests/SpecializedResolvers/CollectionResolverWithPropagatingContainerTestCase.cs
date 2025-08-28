// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
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
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Context;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Resolvers;
using Castle.Windsor.Windsor;
using Castle.Windsor.Windsor.Installer;
using Castle.Windsor.Windsor.Proxy;

namespace Castle.Windsor.Tests.SpecializedResolvers;

public class CollectionResolverWithPropagatingContainerTestCase() :
    CollectionResolverTestCase(
        new WindsorContainer(
            new DefaultKernel(
                new InlineDependenciesPropagatingDependencyResolver(),
                new DefaultProxyFactory()),
            new DefaultComponentInstaller()))
{
    [Fact]
    public void collection_sub_resolver_should_honor_composition_context_if_kernel_propagates_inline_dependencies()
    {
        Container.Register(Component.For<ComponentA>().LifestyleTransient());
        Container.Register(Component.For<IComponentB>().ImplementedBy<ComponentB1>().LifestyleTransient());
        Container.Register(Component.For<IComponentB>().ImplementedBy<ComponentB2>().LifestyleTransient());

        var additionalArguments = Arguments.FromProperties(new { greeting = "Hello propagating system." });
        var componentA = Kernel.Resolve<ComponentA>(additionalArguments);
        Assert.NotNull(componentA);
        Assert.NotNull(componentA.Greeting);
        Assert.NotNull(componentA.ComponentsOfB);
        Assert.Equal(2, componentA.ComponentsOfB.Length);
        foreach (var componentB in componentA.ComponentsOfB)
        {
            Assert.Equal(componentB.Greeting, componentA.Greeting);
        }
    }

    private class InlineDependenciesPropagatingDependencyResolver
        : DefaultDependencyResolver
    {
        protected override CreationContext RebuildContextForParameter(
            CreationContext current,
            Type parameterType)
        {
            return parameterType.GetTypeInfo().ContainsGenericParameters
                ? current
                : new CreationContext(parameterType, current, true);
        }
    }

    public class ComponentA(
        IKernel kernel,
        IComponentB[] componentsOfB,
        string greeting)
    {
        public IComponentB[] ComponentsOfB { get; } = componentsOfB;
        public string Greeting { get; } = greeting;
    }

    public interface IComponentB
    {
        string Greeting { get; }
    }

    public abstract class ComponentB(string greeting) : IComponentB
    {
        public string Greeting { get; } = greeting;
    }

    public class ComponentB1(string greeting) : ComponentB(greeting);

    public class ComponentB2(string greeting) : ComponentB(greeting);
}