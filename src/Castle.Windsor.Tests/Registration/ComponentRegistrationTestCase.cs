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

namespace Castle.Windsor.Tests.Registration;

using System;
using System.Collections.Generic;
using System.Linq;

using Castle.Core.Configuration;
using Castle.DynamicProxy;
using Castle.Windsor.Core;
using Castle.Windsor.Facilities.Startable;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Proxy;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Config.Components;
using Castle.Windsor.Tests.Facilities.Startable.Components;
using Castle.Windsor.Tests.Interceptors;

public class ComponentRegistrationTestCase : AbstractContainerTestCase
{
	[Fact]
	public void AddComponent_WhichIsNull_ThrowsNullArgumentException()
	{
		// Previously the kernel assummed everything was OK, and null reffed instead.
		Assert.Throws<ArgumentNullException>(() => Kernel.Register(Component.For(Type.GetType("NonExistentType, WohooAssembly"))));
	}

	[Fact]
	public void AddComponent_WithServiceOnly_RegisteredWithServiceTypeName()
	{
		Kernel.Register(
			Component.For<CustomerImpl>());

		var handler = Kernel.GetHandler(typeof(CustomerImpl));
		Assert.Equal(typeof(CustomerImpl), handler.ComponentModel.Services.Single());
		Assert.Equal(typeof(CustomerImpl), handler.ComponentModel.Implementation);

		var customer = Kernel.Resolve<CustomerImpl>();
		Assert.NotNull(customer);

		var key = typeof(CustomerImpl).FullName;
		var customer1 = Kernel.Resolve<object>(key);
		Assert.NotNull(customer1);
		Assert.Same(customer, customer1);
	}

	[Fact]
	public void AddComponent_WithInterceptorSelector_ComponentModelShouldHaveInterceptorSelector()
	{
		var selector = new InterceptorTypeSelector(typeof(TestInterceptor1));
		Kernel.Register(Component.For<ICustomer>().Interceptors(new InterceptorReference(typeof(TestInterceptor1)))
			.SelectedWith(selector).Anywhere);

		var handler = Kernel.GetHandler(typeof(ICustomer));

		var proxyOptions = handler.ComponentModel.ObtainProxyOptions(false);

		Assert.NotNull(proxyOptions);
		Assert.Equal(selector, proxyOptions.Selector.Resolve(null, null));
	}

	[Fact]
	public void AddComponent_WithInterfaceServiceOnly_And_Interceptors_ProxyOptionsShouldNotHaveATarget()
	{
		Kernel.Register(
			Component.For<ICustomer>().Interceptors(new InterceptorReference(typeof(StandardInterceptor))).Anywhere);

		var handler = Kernel.GetHandler(typeof(ICustomer));

		var proxyOptions = handler.ComponentModel.ObtainProxyOptions(false);

		Assert.NotNull(proxyOptions);
		Assert.True(proxyOptions.OmitTarget);
	}

	[Fact]
	public void AddComponent_WithServiceAndName_RegisteredNamed()
	{
		Kernel.Register(
			Component.For<CustomerImpl>()
				.Named("customer")
		);

		var handler = Kernel.GetHandler("customer");
		Assert.Equal("customer", handler.ComponentModel.Name);
		Assert.Equal(typeof(CustomerImpl), handler.ComponentModel.Services.Single());
		Assert.Equal(typeof(CustomerImpl), handler.ComponentModel.Implementation);

		var customer = Kernel.Resolve<CustomerImpl>("customer");
		Assert.NotNull(customer);
	}

	[Fact]
	public void AddComponent_NamedAlreadyAssigned_ThrowsException()
	{
		var expectedMessage = "This component has already been assigned name 'customer'";
		var exception = Assert.Throws<ComponentRegistrationException>(() =>
		{
			Kernel.Register(
				Component.For<CustomerImpl>()
					.Named("customer")
					.Named("customer1")
			);
		});
		Assert.Equal(exception.Message, expectedMessage);
	}

	[Fact]
	public void AddComponent_WithSameName_ThrowsException()
	{
		var expectedMessage = "Component customer could not be registered. There is already a component with that name. Did you want to modify the existing component instead? If not, make sure you specify a unique name.";
		var exception = Assert.Throws<ComponentRegistrationException>(() =>
		{
			Kernel.Register(
				Component.For<CustomerImpl>()
					.Named("customer"),
				Component.For<CustomerImpl>()
					.Named("customer")
			);
		});
		Assert.Equal(exception.Message, expectedMessage);
	}

	[Fact]
	public void AddComponent_WithServiceAndClass_RegisteredWithClassTypeName()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.ImplementedBy<CustomerImpl>());

		var customer = Kernel.Resolve<ICustomer>();
		Assert.NotNull(customer);

		var key = typeof(CustomerImpl).FullName;
		var customer1 = Kernel.Resolve<object>(key);
		Assert.NotNull(customer1);
	}

	[Fact]
	public void AddComponent_WithImplementationAlreadyAssigned_ThrowsException()
	{
		var expectedMessage = "This component has already been assigned implementation Castle.Windsor.Tests.ClassComponents.CustomerImpl";
		var exception = Assert.Throws<ComponentRegistrationException>(() =>
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.ImplementedBy<CustomerImpl>()
					.ImplementedBy<CustomerImpl2>()
			);
		});
		Assert.Equal(exception.Message, expectedMessage);
	}

	[Fact]
	public void AddComponent_Instance_UsesInstance()
	{
		var customer = new CustomerImpl();

		Kernel.Register(
			Component.For<ICustomer>()
				.Named("key")
				.Instance(customer)
		);
		Assert.True(Kernel.HasComponent("key"));
		var handler = Kernel.GetHandler("key");
		Assert.Equal(customer.GetType(), handler.ComponentModel.Implementation);

		var customer2 = Kernel.Resolve<ICustomer>("key");
		Assert.Same(customer, customer2);

		customer2 = Kernel.Resolve<ICustomer>();
		Assert.Same(customer, customer2);
	}

	[Fact]
	public void AddComponent_Instance_UsesInstanceWithParameters()
	{
		var customer = new CustomerImpl2("ernst", "delft", 29);

		Kernel.Register(
			Component.For<ICustomer>()
				.Named("key")
				.Instance(customer)
		);
		Assert.True(Kernel.HasComponent("key"));
		var handler = Kernel.GetHandler("key");
		Assert.Equal(customer.GetType(), handler.ComponentModel.Implementation);

		var customer2 = Kernel.Resolve<ICustomer>("key");
		Assert.Same(customer, customer2);

		customer2 = Kernel.Resolve<ICustomer>();
		Assert.Same(customer, customer2);
	}

	[Fact]
	public void AddComponent_WithExplicitLifestyle_WorksFine()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.Named("customer")
				.ImplementedBy<CustomerImpl>()
				.LifeStyle.Is(LifestyleType.Transient)
		);

		var handler = Kernel.GetHandler("customer");
		Assert.Equal(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
	}

	[Fact]
	public void AddComponent_WithTransientLifestyle_WorksFine()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.Named("customer")
				.ImplementedBy<CustomerImpl>()
				.LifeStyle.Transient
		);

		var handler = Kernel.GetHandler("customer");
		Assert.Equal(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
	}

	[Fact]
	public void AddComponent_WithSingletonLifestyle_WorksFine()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.Named("customer")
				.ImplementedBy<CustomerImpl>()
				.LifeStyle.Singleton
		);

		var handler = Kernel.GetHandler("customer");
		Assert.Equal(LifestyleType.Singleton, handler.ComponentModel.LifestyleType);
	}

	[Fact]
	public void AddComponent_WithCustomLifestyle_WorksFine()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.Named("customer")
				.ImplementedBy<CustomerImpl>()
				.LifeStyle.Custom<CustomLifestyleManager>()
		);

		var handler = Kernel.GetHandler("customer");
		Assert.Equal(LifestyleType.Custom, handler.ComponentModel.LifestyleType);
	}

	[Fact]
	public void AddComponent_WithThreadLifestyle_WorksFine()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.Named("customer")
				.ImplementedBy<CustomerImpl>()
				.LifeStyle.PerThread
		);

		var handler = Kernel.GetHandler("customer");
		Assert.Equal(LifestyleType.Thread, handler.ComponentModel.LifestyleType);
	}

	[Fact]
	public void AddComponent_WithPooledLifestyle_WorksFine()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.Named("customer")
				.ImplementedBy<CustomerImpl>()
				.LifeStyle.Pooled
		);

		var handler = Kernel.GetHandler("customer");
		Assert.Equal(LifestyleType.Pooled, handler.ComponentModel.LifestyleType);
	}

	[Fact]
	public void AddComponent_WithPooledWithSizeLifestyle_WorksFine()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.Named("customer")
				.ImplementedBy<CustomerImpl>()
				.LifeStyle.PooledWithSize(5, 10)
		);

		var handler = Kernel.GetHandler("customer");
		Assert.Equal(LifestyleType.Pooled, handler.ComponentModel.LifestyleType);
	}

	[Fact]
	public void AddComponent_Activator_WorksFine()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.Named("customer")
				.ImplementedBy<CustomerImpl>()
				.Activator<MyCustomerActivator>()
		);

		var handler = Kernel.GetHandler("customer");
		Assert.Equal(typeof(MyCustomerActivator), handler.ComponentModel.CustomComponentActivator);

		var customer = Kernel.Resolve<ICustomer>();
		Assert.Equal("James Bond", customer.Name);
	}

	[Fact]
	public void AddComponent_ExtendedProperties_WorksFine()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.ImplementedBy<CustomerImpl>()
				.ExtendedProperties(
					Property.ForKey("key1").Eq("value1"),
					Property.ForKey("key2").Eq("value2")
				)
		);

		var handler = Kernel.GetHandler(typeof(ICustomer));
		Assert.Equal("value1", handler.ComponentModel.ExtendedProperties["key1"]);
		Assert.Equal("value2", handler.ComponentModel.ExtendedProperties["key2"]);
	}

	[Fact]
	public void AddComponent_ExtendedProperties_UsingAnonymousType()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.ImplementedBy<CustomerImpl>()
				.ExtendedProperties(
					Property.ForKey("key1").Eq("value1"),
					Property.ForKey("key2").Eq("value2")));

		var handler = Kernel.GetHandler(typeof(ICustomer));
		Assert.Equal("value1", handler.ComponentModel.ExtendedProperties["key1"]);
		Assert.Equal("value2", handler.ComponentModel.ExtendedProperties["key2"]);
	}

	[Fact]
	public void AddComponent_CustomDependencies_WorksFine()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.ImplementedBy<CustomerImpl>()
				.DependsOn(
					Property.ForKey("Name").Eq("Caption Hook"),
					Property.ForKey("Address").Eq("Fairyland"),
					Property.ForKey("Age").Eq(45)
				)
		);

		var customer = Kernel.Resolve<ICustomer>();
		Assert.Equal("Caption Hook", customer.Name);
		Assert.Equal("Fairyland", customer.Address);
		Assert.Equal(45, customer.Age);
	}

	[Fact]
	public void AddComponent_CustomDependencies_UsingAnonymousType()
	{
		Kernel.Register(
			Component.For<ICustomer>()
				.ImplementedBy<CustomerImpl>()
				.DependsOn(Property.ForKey("Name").Eq("Caption Hook"),
					Property.ForKey("Address").Eq("Fairyland"),
					Property.ForKey("Age").Eq(45)));

		var customer = Kernel.Resolve<ICustomer>();
		Assert.Equal("Caption Hook", customer.Name);
		Assert.Equal("Fairyland", customer.Address);
		Assert.Equal(45, customer.Age);
	}

	[Fact]
	public void AddComponent_CustomDependenciesDictionary_WorksFine()
	{
		var customDependencies = new Dictionary<string, object>();
		customDependencies["Name"] = "Caption Hook";
		customDependencies["Address"] = "Fairyland";
		customDependencies["Age"] = 45;

		Kernel.Register(
			Component.For<ICustomer>()
				.ImplementedBy<CustomerImpl>()
				.DependsOn(customDependencies)
		);

		var customer = Kernel.Resolve<ICustomer>();
		Assert.Equal("Caption Hook", customer.Name);
		Assert.Equal("Fairyland", customer.Address);
		Assert.Equal(45, customer.Age);
	}

	[Fact]
	public void AddComponent_ArrayConfigurationParameters_WorksFine()
	{
		var list = new MutableConfiguration("list");
		list.Attributes.Add("type", typeof(ICommon).AssemblyQualifiedName);
		list.Children.Add(new MutableConfiguration("item", "${common1}"));
		list.Children.Add(new MutableConfiguration("item", "${common2}"));

		Kernel.Register(
			Component.For<ICommon>()
				.Named("common1")
				.ImplementedBy<CommonImpl1>(),
			Component.For<ICommon>()
				.Named("common2")
				.ImplementedBy<CommonImpl2>(),
			Component.For<ClassWithArrayConstructor>()
				.DependsOn(
					Parameter.ForKey("first").Eq("${common2}"),
					Dependency.OnConfigValue("services", list)
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
	public void AddComponent_ListConfigurationParameters_WorksFine()
	{
		var list = new MutableConfiguration("list");
		list.Attributes.Add("type", typeof(ICommon).AssemblyQualifiedName);
		list.Children.Add(new MutableConfiguration("item", "${common1}"));
		list.Children.Add(new MutableConfiguration("item", "${common2}"));

		Kernel.Register(
			Component.For<ICommon>()
				.Named("common1")
				.ImplementedBy<CommonImpl1>(),
			Component.For<ICommon>()
				.Named("common2")
				.ImplementedBy<CommonImpl2>(),
			Component.For<ClassWithListConstructor>()
				.DependsOn(
					Parameter.ForKey("services").Eq(list)
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
	public void AddComponent_WithComplexConfiguration_WorksFine()
	{
		Kernel.Register(
			Component.For<ClassWithComplexParameter>()
				.Configuration(
					Child.ForName("parameters").Eq(
						Attrib.ForName("notUsed").Eq(true),
						Child.ForName("complexparam").Eq(
							Child.ForName("complexparametertype").Eq(
								Child.ForName("mandatoryvalue").Eq("value1"),
								Child.ForName("optionalvalue").Eq("value2")
							)
						)
					)
				)
		);

		var component = Kernel.Resolve<ClassWithComplexParameter>();
		Assert.NotNull(component);
		Assert.NotNull(component.ComplexParam);
		Assert.Equal("value1", component.ComplexParam.MandatoryValue);
		Assert.Equal("value2", component.ComplexParam.OptionalValue);
	}

	[Fact]
	public void AddGenericComponent_WithParameters()
	{
		Kernel.Register(Component.For(typeof(IGenericClassWithParameter<>))
			.ImplementedBy(typeof(GenericClassWithParameter<>))
			.DependsOn(Parameter.ForKey("name").Eq("NewName"))
		);

		var instance = Kernel.Resolve<IGenericClassWithParameter<int>>();
		Assert.Equal("NewName", instance.Name);
	}

	[Fact]
	public void AddComponent_StartableWithInterface_StartsComponent()
	{
		Kernel.AddFacility<StartableFacility>()
			.Register(Component.For<StartableComponent>());

		var component = Kernel.Resolve<StartableComponent>();

		Assert.NotNull(component);
		Assert.True(component.Started);
		Assert.False(component.Stopped);

		Kernel.ReleaseComponent(component);
		Assert.True(component.Stopped);
	}

	[Fact]
	public void AddComponent_StartableWithoutInterface_StartsComponent()
	{
		Kernel.AddFacility<StartableFacility>()
			.Register(Component.For<NoInterfaceStartableComponent>()
				.StartUsingMethod("Start")
				.StopUsingMethod("Stop")
			);

		var component = Kernel.Resolve<NoInterfaceStartableComponent>();

		Assert.NotNull(component);
		Assert.True(component.Started);
		Assert.False(component.Stopped);

		Kernel.ReleaseComponent(component);
		Assert.True(component.Stopped);
	}

	[Fact]
	public void AddComponent_StartableWithoutInterface_StartsComponent_via_expression()
	{
		Kernel.AddFacility<StartableFacility>()
			.Register(Component.For<NoInterfaceStartableComponent>()
				.StartUsingMethod(x => x.Start)
				.StopUsingMethod(x => x.Stop)
			);

		var component = Kernel.Resolve<NoInterfaceStartableComponent>();

		Assert.NotNull(component);
		Assert.True(component.Started);
		Assert.False(component.Stopped);

		Kernel.ReleaseComponent(component);
		Assert.True(component.Stopped);
	}
}