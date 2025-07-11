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
using Castle.Windsor.Tests.Facilities.TypedFactory.Components;
using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;

namespace Castle.Windsor.Tests.Facilities.TypedFactory;

public class ExternalConfigurationTestCase : AbstractContainerTestCase
{
	protected override void AfterContainerCreated()
	{
		var path = ConfigHelper.ResolveConfigPath("Facilities/TypedFactory/typedFactory_castle_config.xml");

		Container.Install(Castle.Windsor.Installer.Configuration.FromXmlFile(path));

		Container.Register(
			Component.For<IProtocolHandler>().ImplementedBy<MirandaProtocolHandler>().Named("miranda"),
			Component.For<IProtocolHandler>().ImplementedBy<MessengerProtocolHandler>().Named("messenger"),
			Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("comp1"),
			Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("comp2"));
	}

	[Fact]
	public void Factory1()
	{
		var factory = Container.Resolve<IProtocolHandlerFactory1>("protocolFac1");

		var handler = factory.Create();

		Assert.NotNull(handler);

		factory.Release(handler);
	}

	[Fact]
	public void Factory2()
	{
		var factory = Container.Resolve<IProtocolHandlerFactory2>("protocolFac2");

		var handler = factory.Create("miranda");
		Assert.NotNull(handler);
		Assert.IsType<MirandaProtocolHandler>(handler);

		factory.Release(handler);

		handler = factory.Create("messenger");
		Assert.NotNull(handler);
		Assert.IsType<MessengerProtocolHandler>(handler);

		factory.Release(handler);
	}

	[Fact]
	public void Factory3()
	{
		var factory = Container.Resolve<IComponentFactory1>("compFactory1");

		var comp1 = factory.Construct();
		Assert.NotNull(comp1);

		var comp2 = factory.Construct();
		Assert.NotNull(comp2);
	}

	[Fact]
	public void Factory4()
	{
		var factory = Container.Resolve<IComponentFactory2>("compFactory2");

		var comp1 = (IDummyComponent)factory.Construct("comp1");
		Assert.IsType<Component1>(comp1);
		Assert.NotNull(comp1);

		var comp2 = (IDummyComponent)factory.Construct("comp2");
		Assert.IsType<Component2>(comp2);
		Assert.NotNull(comp2);
	}

	[Fact]
	public void No_Creation_Or_Destruction_methods_defined()
	{
		var factory = Container.Resolve<IComponentFactory1>("NoCreationOrDestructionDefined");

		Assert.NotNull(factory);

		var comp1 = factory.Construct();
		Assert.NotNull(comp1);

		var comp2 = factory.Construct();
		Assert.NotNull(comp2);
	}

	[Fact]
	public void Selector_in_xml()
	{
		Container.Register(
			Component.For<IDummyComponent>().ImplementedBy<Component1>(),
			Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("one"));
		var factory = Container.Resolve<IComponentFactory1>("HasOneSelector");
		var dummyComponent = factory.Construct();
		Assert.IsType<Component2>(dummyComponent);
	}
}