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

using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests.Registration;

public class ServiceOverridesTestCase : AbstractContainerTestCase
{
	[Fact]
	public void AddComponent_ArrayServiceOverrides_WorksFine()
	{
		Kernel.Register(
			Component.For<ICommon>()
				.Named("common1")
				.ImplementedBy<CommonImpl1>(),
			Component.For<ICommon>()
				.Named("common2")
				.ImplementedBy<CommonImpl2>(),
			Component.For<ClassWithArrayConstructor>()
				.DependsOn(
					ServiceOverride.ForKey("first").Eq("common2"),
					ServiceOverride.ForKey("services").Eq("common1", "common2")
				)
		);

		var common1 = Kernel.Resolve<ICommon>("common1");
		var common2 = Kernel.Resolve<ICommon>("common2");
		var component = Kernel.Resolve<ClassWithArrayConstructor>();
		Assert.Same(common2, component.First);
		Assert.Equal(2, component.Services.Length);
		Assert.Same(common1, component.Services[0]);
		Assert.Same(common2, component.Services[1]);
	}

	[Fact]
	public void AddComponent_GenericListServiceOverrides_WorksFine()
	{
		Kernel.Register(
			Component.For<ICommon>()
				.Named("common1")
				.ImplementedBy<CommonImpl1>(),
			Component.For<ICommon>()
				.Named("common2")
				.ImplementedBy<CommonImpl2>(),
			Component.For<ClassWithListConstructor>()
				.DependsOn(
					ServiceOverride.ForKey("services").Eq<ICommon>("common1", "common2")
				)
		);

		var common1 = Kernel.Resolve<ICommon>("common1");
		var common2 = Kernel.Resolve<ICommon>("common2");
		var component = Kernel.Resolve<ClassWithListConstructor>();
		Assert.Equal(2, component.Services.Count);
		Assert.Same(common1, component.Services[0]);
		Assert.Same(common2, component.Services[1]);
	}

	[Fact]
	public void AddComponent_DependencyOnComponent_WorksFine()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.Named("customer1")
				.ImplementedBy<CustomerImpl>()
				.DependsOn(
					Property.ForKey("Name").Eq("Caption Hook"),
					Property.ForKey("Address").Eq("Fairyland"),
					Property.ForKey("Age").Eq(45)
				),
			Component.For<CustomerChain1>()
				.Named("customer2")
				.DependsOn(
					Property.ForKey("Name").Eq("Bigfoot"),
					Property.ForKey("Address").Eq("Forest"),
					Property.ForKey("Age").Eq(100)
				)
				.DependsOn(Dependency.OnComponent("customer", "customer1"))
		);

		var customer = Kernel.Resolve<CustomerChain1>("customer2");
		Assert.NotNull(customer.CustomerBase);
		Assert.Equal("Caption Hook", customer.CustomerBase.Name);
		Assert.Equal("Fairyland", customer.CustomerBase.Address);
		Assert.Equal(45, customer.CustomerBase.Age);
	}

	[Fact]
	public void AddComponent_ServiceOverrides_UsingAnonymousType()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.Named("customer1")
				.ImplementedBy<CustomerImpl>()
				.DependsOn(
					Property.ForKey("Name").Eq("Caption Hook"),
					Property.ForKey("Address").Eq("Fairyland"),
					Property.ForKey("Age").Eq(45)
				),
			Component.For<CustomerChain1>()
				.Named("customer2")
				.DependsOn(
					Property.ForKey("Name").Eq("Bigfoot"),
					Property.ForKey("Address").Eq("Forest"),
					Property.ForKey("Age").Eq(100)
				)
				.DependsOn(
					ServiceOverride.ForKey("customer").Eq("customer1"))
		);

		var customer = Kernel.Resolve<CustomerChain1>("customer2");
		Assert.NotNull(customer.CustomerBase);
		Assert.Equal("Caption Hook", customer.CustomerBase.Name);
		Assert.Equal("Fairyland", customer.CustomerBase.Address);
		Assert.Equal(45, customer.CustomerBase.Age);
	}

	[Fact]
	public void AddComponent_ServiceOverrides_WorksFine()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.Named("customer1")
				.ImplementedBy<CustomerImpl>()
				.DependsOn(
					Property.ForKey("Name").Eq("Caption Hook"),
					Property.ForKey("Address").Eq("Fairyland"),
					Property.ForKey("Age").Eq(45)
				),
			Component.For<CustomerChain1>()
				.Named("customer2")
				.DependsOn(
					Property.ForKey("Name").Eq("Bigfoot"),
					Property.ForKey("Address").Eq("Forest"),
					Property.ForKey("Age").Eq(100)
				)
				.DependsOn(
					ServiceOverride.ForKey("customer").Eq("customer1")
				)
		);

		var customer = Kernel.Resolve<CustomerChain1>("customer2");
		Assert.NotNull(customer.CustomerBase);
		Assert.Equal("Caption Hook", customer.CustomerBase.Name);
		Assert.Equal("Fairyland", customer.CustomerBase.Address);
		Assert.Equal(45, customer.CustomerBase.Age);
	}

	[Fact]
	public void ServiceOverrides_work_via_DependsOn_named_key()
	{
		Kernel.Register(
			Component.For<IEmptyService>()
				.Named("customer1")
				.ImplementedBy<EmptyServiceA>(),
			Component.For<IEmptyService>()
				.Named("customer2")
				.ImplementedBy<EmptyServiceB>(),
			Component.For<UsesIEmptyService>()
				.DependsOn(Property.ForKey("emptyService").Is("customer2"))
		);

		var service = Kernel.Resolve<UsesIEmptyService>();
		Assert.IsType<EmptyServiceB>(service.EmptyService);
	}

	[Fact]
	public void ServiceOverrides_work_via_DependsOn_named_key_typed_value_generic()
	{
		Kernel.Register(
			Component.For<IEmptyService>()
				.ImplementedBy<EmptyServiceA>(),
			Component.For<IEmptyService>()
				.ImplementedBy<EmptyServiceB>(),
			Component.For<UsesIEmptyService>()
				.DependsOn(Property.ForKey("emptyService").Is<EmptyServiceB>())
		);

		var service = Kernel.Resolve<UsesIEmptyService>();
		Assert.IsType<EmptyServiceB>(service.EmptyService);
	}

	[Fact]
	public void ServiceOverrides_work_via_DependsOn_named_key_typed_value_nongeneric()
	{
		Kernel.Register(
			Component.For<IEmptyService>()
				.ImplementedBy<EmptyServiceA>(),
			Component.For<IEmptyService>()
				.ImplementedBy<EmptyServiceB>(),
			Component.For<UsesIEmptyService>()
				.DependsOn(Property.ForKey("emptyService").Is(typeof(EmptyServiceB)))
		);

		var service = Kernel.Resolve<UsesIEmptyService>();
		Assert.IsType<EmptyServiceB>(service.EmptyService);
	}

	[Fact]
	public void ServiceOverrides_work_via_DependsOn_typed_key()
	{
		Kernel.Register(
			Component.For<IEmptyService>()
				.Named("customer1")
				.ImplementedBy<EmptyServiceA>(),
			Component.For<IEmptyService>()
				.Named("customer2")
				.ImplementedBy<EmptyServiceB>(),
			Component.For<UsesIEmptyService>()
				.DependsOn(Property.ForKey<IEmptyService>().Is("customer2"))
		);

		var service = Kernel.Resolve<UsesIEmptyService>();
		Assert.IsType<EmptyServiceB>(service.EmptyService);
	}

	[Fact]
	public void ServiceOverrides_works_via_DependsOn_typed_key_Named_value_on_open_generic_type()
	{
		Kernel.Register(
			Component.For<IEmptyService>().UsingFactoryMethod(() => new EmptyServiceA()).Named("a"),
			Component.For<IEmptyService>().UsingFactoryMethod(() => new EmptyServiceB()).Named("b"),
			Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl3<>))
				.DependsOn(Property.ForKey<IEmptyService>().Is("B")));

		var root = (GenericImpl3<int>)Kernel.Resolve<IGeneric<int>>();

		Assert.IsType<EmptyServiceB>(root.Value);
	}
}