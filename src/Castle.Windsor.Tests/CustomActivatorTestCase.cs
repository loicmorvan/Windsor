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

namespace Castle.Windsor.Tests;

using System;

using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;

public class CustomActivatorTestCase:IDisposable
{
	private readonly IKernel kernel = new DefaultKernel();

	[Fact]
	public void Can_resolve_component_with_primitive_dependency_via_factory()
	{
		kernel.Register(
			Component.For<ClassWithPrimitiveDependency>()
				.UsingFactoryMethod(() => new ClassWithPrimitiveDependency(2)));

		kernel.Resolve<ClassWithPrimitiveDependency>();
	}

	[Fact]
	public void Can_resolve_component_with_primitive_dependency_via_instance()
	{
		kernel.Register(
			Component.For<ClassWithPrimitiveDependency>()
				.Instance(new ClassWithPrimitiveDependency(2)));

		kernel.Resolve<ClassWithPrimitiveDependency>();
	}

	[Fact]
	public void Can_resolve_component_with_service_dependency_via_factory()
	{
		kernel.Register(
			Component.For<ClassWithServiceDependency>()
				.UsingFactoryMethod(() => new ClassWithServiceDependency(null)));

		kernel.Resolve<ClassWithServiceDependency>();
	}

	[Fact]
	public void Can_resolve_component_with_service_dependency_via_instance()
	{
		kernel.Register(
			Component.For<ClassWithServiceDependency>()
				.Instance(new ClassWithServiceDependency(null)));

		kernel.Resolve<ClassWithServiceDependency>();
	}

	public void Dispose()
	{
		kernel.Dispose();
	}
}

public class ClassWithPrimitiveDependencyFactory
{
	public ClassWithPrimitiveDependency Create()
	{
		return new ClassWithPrimitiveDependency(2);
	}
}

public class ClassWithPrimitiveDependency(int dependency)
{
	private int dependency = dependency;
}

public class ClassWithServiceDependency(IService dependency)
{
	private IService dependency = dependency;
}