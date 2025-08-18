// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

// ReSharper disable UnusedParameter.Local

namespace Castle.Windsor.Tests;

public sealed class CustomActivatorTestCase : IDisposable
{
    private readonly DefaultKernel _kernel = new();

    public void Dispose()
    {
        _kernel.Dispose();
    }

    [Fact]
    public void Can_resolve_component_with_primitive_dependency_via_factory()
    {
        _kernel.Register(
            Component.For<ClassWithPrimitiveDependency>()
                .UsingFactoryMethod(() => new ClassWithPrimitiveDependency(2)));

        _kernel.Resolve<ClassWithPrimitiveDependency>();
    }

    [Fact]
    public void Can_resolve_component_with_primitive_dependency_via_instance()
    {
        _kernel.Register(
            Component.For<ClassWithPrimitiveDependency>()
                .Instance(new ClassWithPrimitiveDependency(2)));

        _kernel.Resolve<ClassWithPrimitiveDependency>();
    }

    [Fact]
    public void Can_resolve_component_with_service_dependency_via_factory()
    {
        _kernel.Register(
            Component.For<ClassWithServiceDependency>()
                .UsingFactoryMethod(() => new ClassWithServiceDependency(null)));

        _kernel.Resolve<ClassWithServiceDependency>();
    }

    [Fact]
    public void Can_resolve_component_with_service_dependency_via_instance()
    {
        _kernel.Register(
            Component.For<ClassWithServiceDependency>()
                .Instance(new ClassWithServiceDependency(null)));

        _kernel.Resolve<ClassWithServiceDependency>();
    }
}