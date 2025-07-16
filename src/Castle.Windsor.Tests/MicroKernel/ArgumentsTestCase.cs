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

namespace Castle.Windsor.Tests.MicroKernel;

using System;
using System.Collections.Generic;

using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;

using CastleTests;
using CastleTests.Components;

public class ArgumentsTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Any_type_as_key_is_not_supported()
	{
		var arguments = new Arguments();

		Assert.Throws<ArgumentException>(delegate { arguments.Add(123, 321); });
		Assert.Throws<ArgumentException>(delegate { arguments.Add(new object(), "value"); });
	}

	[Fact]
	[Bug("IOC-147")]
	public void Can_have_dictionary_as_inline_dependency()
	{
		var container = new WindsorContainer();
		container.Register(Component.For<HasDictionaryDependency>());

		var dictionaryProperty = new Dictionary<string, string>();

		var obj = container.Resolve<HasDictionaryDependency>(Arguments.FromProperties(new { dictionaryProperty }));
		Assert.Same(dictionaryProperty, obj.DictionaryProperty);
	}

	[Fact]
	[Bug("IOC-142")]
	public void Can_satisfy_nullable_property_dependency()
	{
		Container.Register(Component.For<HasNullableIntProperty>());

		var arguments = new Arguments().AddNamed("SomeVal", 5);
		var s = Container.Resolve<HasNullableIntProperty>(arguments);

		Assert.NotNull(s.SomeVal);
	}

	[Fact]
	[Bug("IOC-142")]
	public void Can_satisfy_nullable_ctor_dependency()
	{
		Container.Register(Component.For<HasNullableDoubleConstructor>());

		var s = Container.Resolve<HasNullableDoubleConstructor>(new Arguments().AddNamed("foo", 5d));
		Assert.NotNull(s);
	}

	[Fact]
	[Bug("IOC-92")]
	public void Can_mix_hashtable_parameters_and_configuration_parameters()
	{
		Container.Register(
			Component.For<HasStringAndIntDependency>()
				.DependsOn(Parameter.ForKey("x").Eq("abc"))
		);

		Container.Resolve<HasStringAndIntDependency>(new Arguments().AddNamed("y", 1));
	}

	[Fact]
	public void Handles_Type_as_key()
	{
		var arguments = new Arguments();
		var key = typeof(object);
		var value = new object();

		arguments.Add(key, value);

		Assert.Equal(1, arguments.Count);
		Assert.True(arguments.Contains(key));
		Assert.Same(value, arguments[key]);
	}

	[Fact]
	public void Handles_string_as_key()
	{
		var arguments = new Arguments();
		var key = "Foo";
		var value = new object();

		arguments.Add(key, value);

		Assert.Equal(1, arguments.Count);
		Assert.True(arguments.Contains(key));
		Assert.Same(value, arguments[key]);
	}

	[Fact]
	public void Handles_string_as_key_case_insensitive()
	{
		var arguments = new Arguments();
		var key = "foo";
		var value = new object();

		arguments.Add(key, value);

		Assert.True(arguments.Contains(key.ToLower()));
		Assert.True(arguments.Contains(key.ToUpper()));
	}
}