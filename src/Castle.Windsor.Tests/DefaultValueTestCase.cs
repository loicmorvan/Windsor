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

namespace Castle.Windsor.Tests;

using System.Collections.Generic;

using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Windsor;

public class DefaultValueTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Can_resolve_component_with_default_ctor_value()
	{
		Container.Register(Component.For<CtorWithDefaultValue>());

		Container.Resolve<CtorWithDefaultValue>();
	}

	[Fact]
	public void Can_resolve_component_with_default_ctor_value_null_for_service_dependency()
	{
		Container.Register(Component.For<HasNullDefaultForServiceDependency>());

		var service = Container.Resolve<HasNullDefaultForServiceDependency>();

		Assert.Null(service.Dependency);
	}

	[Fact]
	public void Null_is_a_valid_default_value()
	{
		Container.Register(Component.For<CtorWithNullDefaultValueAndDefault>());

		var value = Container.Resolve<CtorWithNullDefaultValueAndDefault>();

		Assert.Null(value.Name);
	}

	[Fact]
	public void Uses_ctor_with_defaults_when_greediest()
	{
		Container.Register(Component.For<CtorWithDefaultValueAndDefault>());

		var value = Container.Resolve<CtorWithDefaultValueAndDefault>();

		Assert.False(string.IsNullOrEmpty(value.Name));
	}

	[Fact]
	public void Uses_ctor_with_explicit_dependency_when_equally_greedy_as_default_1()
	{
		Container.Register(Component.For<TwoCtorsWithDefaultValue>().DependsOn(Property.ForKey("name").Eq("Adam Mickiewicz")));

		var value = Container.Resolve<TwoCtorsWithDefaultValue>();

		Assert.Equal("Adam Mickiewicz", value.Name);
	}

	[Fact]
	public void Uses_ctor_with_explicit_dependency_when_equally_greedy_as_default_2()
	{
		Container.Register(Component.For<TwoCtorsWithDefaultValue>().DependsOn(Property.ForKey("age").Eq(123)));

		var value = Container.Resolve<TwoCtorsWithDefaultValue>();

		Assert.Equal(123, value.Age);
	}

	[Fact]
	public void Uses_explicit_value_over_default()
	{
		Container.Register(Component.For<CtorWithDefaultValue>().DependsOn(Property.ForKey("name").Eq("Adam Mickiewicz")));

		var value = Container.Resolve<CtorWithDefaultValue>();

		Assert.Equal("Adam Mickiewicz", value.Name);
	}

	[Fact]
	public void First_chance_exceptions_are_not_thrown()
	{
		using var container = new WindsorContainer();
		container.Register(Component.For<HasCtorWithOptionalInterfaceParameter>());

		TestUtils.AssertNoFirstChanceExceptions(() => container.Resolve<HasCtorWithOptionalInterfaceParameter>());
	}

	private sealed class HasCtorWithOptionalInterfaceParameter
	{
		public HasCtorWithOptionalInterfaceParameter(IEqualityComparer<int> comparer = null)
		{
		}
	}
}