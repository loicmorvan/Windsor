// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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
using Castle.Windsor.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor.Windsor.Extensions;

namespace Castle.Windsor.Tests;

public class ResolveArgumentsTestCase : AbstractContainerTestCase
{
    public ResolveArgumentsTestCase()
    {
        Container.Kernel.Resolver.AddSubResolver(new ListResolver(Container.Kernel));
        Container.Register(Component.For<Service>());
        Container.Register(Component.For<IDependencyWithManyImplementations>()
            .ImplementedBy<DependencyImplementationA>());
        Container.Register(Component.For<IDependencyWithManyImplementations>()
            .ImplementedBy<DependencyImplementationB>());
    }

    [Fact]
    public void Can_Resolve_using_Arguments_as_Properties()
    {
        Container.Resolve<Service>(Arguments.FromProperties(new { Dependency = new Dependency() }));
    }

    [Fact]
    public void Can_ResolveAll_using_Arguments_as_Properties()
    {
        Container.ResolveAll<IDependencyWithManyImplementations>(
            Arguments.FromProperties(new { Dependency = new Dependency() }));
    }

    [Fact]
    public void Can_Resolve_using_Type_and_Arguments_as_Properties()
    {
        Container.Resolve<Service>(Arguments.FromProperties(new { Dependency = new Dependency() }));
    }

    [Fact]
    public void Can_ResolveAll_using_Type_and_Arguments_as_Properties()
    {
        Container.ResolveAll<IDependencyWithManyImplementations>(
            Arguments.FromProperties(new { Dependency = new Dependency() }));
    }

    [Fact]
    public void Can_Resolve_using_Arguments_as_Dictionary()
    {
        var dictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
        Container.Resolve<Service>(Arguments.FromNamed(dictionary));
    }

    [Fact]
    public void Can_ResolveAll_using_Arguments_as_Dictionary()
    {
        var dictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
        Container.ResolveAll<IDependencyWithManyImplementations>(Arguments.FromNamed(dictionary));
    }

    [Fact]
    public void Can_Resolve_using_Type_and_Arguments_as_Dictionary()
    {
        var dictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
        Container.Resolve<Service>(Arguments.FromNamed(dictionary));
    }

    [Fact]
    public void Can_ResolveAll_using_Type_and_Arguments_as_Dictionary()
    {
        var dictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
        Container.ResolveAll<IDependencyWithManyImplementations>(new Arguments().AddNamed(dictionary));
    }

    [Fact]
    public void Can_Resolve_using_Arguments_as_ReadOnlyDictionary()
    {
        IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object>
            { { "dependency", new Dependency() } };
        Container.Resolve<Service>(new Arguments().AddNamed(readOnlyDictionary));
    }

    [Fact]
    public void Can_ResolveAll_using_Arguments_as_ReadOnlyDictionary()
    {
        IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object>
            { { "dependency", new Dependency() } };
        Container.ResolveAll<IDependencyWithManyImplementations>(new Arguments().AddNamed(readOnlyDictionary));
    }

    [Fact]
    public void Can_Resolve_using_ReadOnlyDictionary()
    {
        IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object>
            { { "dependency", new Dependency() } };
        Container.Resolve<Service>(readOnlyDictionary);
    }

    [Fact]
    public void Can_ResolveAll_using_ReadOnlyDictionary()
    {
        IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object>
            { { "dependency", new Dependency() } };
        Container.ResolveAll<IDependencyWithManyImplementations>(readOnlyDictionary);
    }

    [Fact]
    public void Can_Resolve_using_Type_and_Arguments_as_ReadOnlyDictionary()
    {
        IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object>
            { { "dependency", new Dependency() } };
        Container.Resolve<Service>(new Arguments().AddNamed(readOnlyDictionary));
    }

    [Fact]
    public void Can_ResolveAll_using_Type_and_Arguments_as_ReadOnlyDictionary()
    {
        IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object>
            { { "dependency", new Dependency() } };
        Container.ResolveAll<IDependencyWithManyImplementations>(new Arguments().AddNamed(readOnlyDictionary));
    }

    [Fact]
    public void Can_Resolve_Type_and_ReadOnlyDictionary()
    {
        IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object>
            { { "dependency", new Dependency() } };
        Container.Resolve<Service>(readOnlyDictionary);
    }

    [Fact]
    public void Can_ResolveAll_Type_and_ReadOnlyDictionary()
    {
        IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object>
            { { "dependency", new Dependency() } };
        Container.ResolveAll<IDependencyWithManyImplementations>(readOnlyDictionary);
    }

    [Fact]
    public void Can_Resolve_using_Arguments_as_TypedComponents()
    {
        Container.Resolve<Service>(new Arguments().AddTyped(new Dependency()));
    }

    [Fact]
    public void Can_ResolveAll_using_Arguments_as_TypedComponents()
    {
        Container.ResolveAll<IDependencyWithManyImplementations>(new Arguments().AddTyped(new Dependency()));
    }

    [Fact]
    public void Can_Resolve_using_Type_and_Arguments_as_TypedComponents()
    {
        Container.Resolve<Service>(new Arguments().AddTyped(new Dependency()));
    }

    [Fact]
    public void Can_ResolveAll_using_Type_and_Arguments_as_TypedComponents()
    {
        Container.Resolve<IDependencyWithManyImplementations>(new Arguments().AddTyped(new Dependency()));
    }

    private class Dependency;

    private class Service
    {
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        public Service(Dependency dependency)
        {
            Assert.NotNull(dependency);
        }
    }

    private interface IDependencyWithManyImplementations;

    private class DependencyImplementationA : IDependencyWithManyImplementations
    {
        public DependencyImplementationA(Dependency dependency)
        {
            Assert.NotNull(dependency);
        }
    }

    private class DependencyImplementationB : IDependencyWithManyImplementations
    {
        public DependencyImplementationB(Dependency dependency)
        {
            Assert.NotNull(dependency);
        }
    }
}