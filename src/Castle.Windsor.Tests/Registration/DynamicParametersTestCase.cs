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

namespace Castle.MicroKernel.Tests.Registration;

using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Tests.ClassComponents;

using CastleTests;
using CastleTests.Components;

public class DynamicParametersTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Arguments_are_case_insensitive_when_using_anonymous_object()
	{
		var wasCalled = false;
		Kernel.Register(Component.For<ClassWithArguments>().LifeStyle.Transient.DynamicParameters((k, d) =>
		{
			Assert.True(d.Contains("ArG1"));
			wasCalled = true;
		}));

		Kernel.Resolve<ClassWithArguments>(new Arguments().AddNamed("arg2", 2).AddNamed("arg1", "foo"));

		Assert.True(wasCalled);
	}

	[Fact]
	public void Can_dynamically_override_services()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.ImplementedBy<CustomerImpl>()
				.Named("defaultCustomer"),
			Component.For<ICustomer>().ImplementedBy<CustomerImpl2>()
				.Named("otherCustomer")
				.DependsOn(
					Parameter.ForKey("name").Eq("foo"), // static parameters, resolved at registration time
					Parameter.ForKey("address").Eq("bar st 13"),
					Parameter.ForKey("age").Eq("5")),
			Component.For<CommonImplWithDependency>()
				.LifeStyle.Transient
				.DynamicParameters((k, d) => // dynamic parameters
				{
					var randomNumber = 2;
					if (randomNumber == 2) d["customer"] = k.Resolve<ICustomer>("otherCustomer");
				}));

		var component = Kernel.Resolve<CommonImplWithDependency>();
		Assert.IsType<CustomerImpl2>(component.Customer);
	}

	[Fact]
	public void Can_mix_registration_and_call_site_parameters()
	{
		Kernel.Register(
			Component.For<ClassWithArguments>().LifeStyle.Transient.DynamicParameters((k, d) => d["arg1"] = "foo"));

		var component = Kernel.Resolve<ClassWithArguments>(new Arguments().AddNamed("arg2", 2));
		Assert.Equal(2, component.Arg2);
		Assert.Equal("foo", component.Arg1);
	}

	[Fact]
	public void Can_release_components_with_dynamic_parameters()
	{
		var releaseCalled = 0;
		Kernel.Register(
			Component.For<ClassWithArguments>().LifeStyle.Transient
				.DynamicParameters((k, d) =>
				{
					d["arg1"] = "foo";
					return kk => ++releaseCalled;
				})
				.DynamicParameters((k, d) => { return kk => ++releaseCalled; }));

		var component = Kernel.Resolve<ClassWithArguments>(new Arguments().AddNamed("arg2", 2));
		Assert.Equal(2, component.Arg2);
		Assert.Equal("foo", component.Arg1);

		Kernel.ReleaseComponent(component);
		Assert.Equal(2, releaseCalled);
	}

	[Fact]
	public void Can_release_generics_with_dynamic_parameters()
	{
		var releaseCalled = 0;
		Kernel.Register(
			Component.For(typeof(IGenericClassWithParameter<>))
				.ImplementedBy(typeof(GenericClassWithParameter<>)).LifeStyle.Transient
				.DynamicParameters((k, d) =>
				{
					d["name"] = "foo";
					return kk => ++releaseCalled;
				})
				.DynamicParameters((k, d) => { return kk => ++releaseCalled; }));

		var component = Kernel.Resolve<IGenericClassWithParameter<int>>(new Arguments().AddNamed("name", "bar"));
		Assert.Equal("foo", component.Name);

		Kernel.ReleaseComponent(component);
		Assert.Equal(2, releaseCalled);
	}

	[Fact]
	public void Should_handle_multiple_calls()
	{
		var arg1 = "bar";
		var arg2 = 5;
		Kernel.Register(Component.For<ClassWithArguments>()
			.LifeStyle.Transient
			.DynamicParameters((k, d) => { d["arg1"] = arg1; })
			.DynamicParameters((k, d) => { d["arg2"] = arg2; }));
		var component = Kernel.Resolve<ClassWithArguments>(new Arguments().AddNamed("arg2", 2).AddNamed("arg1", "foo"));
		Assert.Equal(arg1, component.Arg1);
		Assert.Equal(arg2, component.Arg2);
	}

	[Fact]
	public void Should_have_access_to_parameters_passed_from_call_site()
	{
		string arg1 = null;
		var arg2 = 0;
		Kernel.Register(Component.For<ClassWithArguments>().LifeStyle.Transient.DynamicParameters((k, d) =>
		{
			arg1 = (string)d["arg1"];
			arg2 = (int)d["arg2"];
		}));
		var component = Kernel.Resolve<ClassWithArguments>(new Arguments().AddNamed("arg2", 2).AddNamed("arg1", "foo"));
		Assert.Equal("foo", arg1);
		Assert.Equal(2, arg2);
	}

	[Fact]
	public void Should_not_require_explicit_registration()
	{
		Kernel.Register(Component.For<CommonSub2Impl>().LifeStyle.Transient.DynamicParameters((k, d) => { }));
		Kernel.Resolve<CommonSub2Impl>();
	}

	[Fact]
	public void Should_override_parameters_passed_from_call_site()
	{
		var arg1 = "bar";
		var arg2 = 5;
		Kernel.Register(Component.For<ClassWithArguments>().LifeStyle.Transient.DynamicParameters((k, d) =>
		{
			d["arg1"] = arg1;
			d["arg2"] = arg2;
		}));
		var component = Kernel.Resolve<ClassWithArguments>(new Arguments().AddNamed("arg2", 2).AddNamed("arg1", "foo"));
		Assert.Equal(arg1, component.Arg1);
		Assert.Equal(arg2, component.Arg2);
	}

	[Fact]
	public void Should_resolve_component_when_no_parameters_passed_from_call_site()
	{
		var arg1 = "bar";
		var arg2 = 5;
		Kernel.Register(Component.For<ClassWithArguments>().LifeStyle.Transient.DynamicParameters((k, d) =>
		{
			d["arg1"] = arg1;
			d["arg2"] = arg2;
		}));

		Kernel.Resolve<ClassWithArguments>();
	}
}