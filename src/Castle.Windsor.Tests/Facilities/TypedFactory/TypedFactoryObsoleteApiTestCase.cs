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

using Castle.Windsor.Facilities.TypedFactory;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Facilities.TypedFactory.Components;
using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;

namespace Castle.Windsor.Tests.Facilities.TypedFactory;

public class TypedFactoryObsoleteApiTestCase : AbstractContainerTestCase
{
	private TypedFactoryFacility facility;

	[Fact]
	public void Factory1()
	{
#pragma warning disable 0618 //call to obsolete method
		facility.AddTypedFactoryEntry(
			new FactoryEntry(
				"protocolHandlerFactory", typeof(IProtocolHandlerFactory1), "Create", "Release"));
#pragma warning restore
		Container.Register(
			Component.For(typeof(IProtocolHandler)).ImplementedBy(typeof(MirandaProtocolHandler)).Named("miranda"));
		Container.Register(
			Component.For(typeof(IProtocolHandler)).ImplementedBy(typeof(MessengerProtocolHandler)).Named("messenger"));

		var factory = Container.Resolve<IProtocolHandlerFactory1>("protocolHandlerFactory");

		Assert.NotNull(factory);

		var handler = factory.Create();

		Assert.NotNull(handler);

		factory.Release(handler);
	}

	[Fact]
	public void Factory2()
	{
#pragma warning disable 0618 //call to obsolete method
		facility.AddTypedFactoryEntry(
			new FactoryEntry(
				"protocolHandlerFactory", typeof(IProtocolHandlerFactory2), "Create", "Release"));
#pragma warning restore
		Container.Register(
			Component.For(typeof(IProtocolHandler)).ImplementedBy(typeof(MirandaProtocolHandler)).Named("miranda"));
		Container.Register(
			Component.For(typeof(IProtocolHandler)).ImplementedBy(typeof(MessengerProtocolHandler)).Named("messenger"));

		var factory = Container.Resolve<IProtocolHandlerFactory2>("protocolHandlerFactory");

		Assert.NotNull(factory);

		var handler = factory.Create("miranda");
		Assert.NotNull(handler);
		Assert.True(handler is MirandaProtocolHandler);
		factory.Release(handler);

		handler = factory.Create("messenger");
		Assert.NotNull(handler);
		Assert.True(handler is MessengerProtocolHandler);
		factory.Release(handler);
	}

	[Fact]
	public void Factory3()
	{
#pragma warning disable 0618 //call to obsolete method
		facility.AddTypedFactoryEntry(
			new FactoryEntry(
				"compFactory", typeof(IComponentFactory1), "Construct", ""));
#pragma warning restore
		Container.Register(Component.For(typeof(IDummyComponent)).ImplementedBy(typeof(Component1)).Named("comp1"));
		Container.Register(Component.For(typeof(IDummyComponent)).ImplementedBy(typeof(Component2)).Named("comp2"));

		var factory = Container.Resolve<IComponentFactory1>("compFactory");
		Assert.NotNull(factory);

		var comp1 = factory.Construct();
		Assert.NotNull(comp1);

		var comp2 = factory.Construct();
		Assert.NotNull(comp2);
	}

	[Fact]
	public void Factory4()
	{
#pragma warning disable 0618 //call to obsolete method
		facility.AddTypedFactoryEntry(
			new FactoryEntry(
				"compFactory", typeof(IComponentFactory2), "Construct", ""));
#pragma warning restore

		Container.Register(Component.For(typeof(IDummyComponent)).ImplementedBy(typeof(Component1)).Named("comp1"));
		Container.Register(Component.For(typeof(IDummyComponent)).ImplementedBy(typeof(Component2)).Named("comp2"));

		var factory = Container.Resolve<IComponentFactory2>("compFactory");

		Assert.NotNull(factory);

		var comp1 = (IDummyComponent)factory.Construct("comp1");
		Assert.True(comp1 is Component1);
		Assert.NotNull(comp1);

		var comp2 = (IDummyComponent)factory.Construct("comp2");
		Assert.True(comp2 is Component2);
		Assert.NotNull(comp2);
	}

	protected override void AfterContainerCreated()
	{
		facility = new TypedFactoryFacility();
		Container.AddFacility(facility);
	}
}