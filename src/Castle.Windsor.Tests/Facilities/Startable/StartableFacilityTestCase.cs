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

using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.Core.Configuration;
using Castle.Windsor.Core;
using Castle.Windsor.Facilities.Startable;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.ModelBuilder;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Facilities.Startable.Components;

namespace Castle.Windsor.Tests.Facilities.Startable;

public class StartableFacilityTestCase
{
	private readonly Assembly _currentAssembly = typeof(StartableFacilityTestCase).GetTypeInfo().Assembly;

	private readonly IKernel _kernel;

	private bool _startableCreatedBeforeResolved;

	public StartableFacilityTestCase()
	{
		_kernel = new DefaultKernel();

		_startableCreatedBeforeResolved = false;
	}

	private void OnNoInterfaceStartableComponentStarted(ComponentModel mode, object instance)
	{
		var startable = instance as NoInterfaceStartableComponent;

		Assert.NotNull(startable);
		Assert.True(startable.Started);
		Assert.False(startable.Stopped);

		_startableCreatedBeforeResolved = true;
	}

	private void OnStartableComponentStarted(ComponentModel mode, object instance)
	{
		var startable = instance as StartableComponent;

		Assert.NotNull(startable);
		Assert.True(startable.Started);
		Assert.False(startable.Stopped);

		_startableCreatedBeforeResolved = true;
	}

	[Fact]
	public void Startable_with_throwing_property_dependency()
	{
		HasThrowingPropertyDependency.InstancesStarted = 0;
		HasThrowingPropertyDependency.InstancesCreated = 0;
		_kernel.AddFacility<StartableFacility>();
		_kernel.Register(
			Component.For<ThrowsInCtor>(),
			Component.For<HasThrowingPropertyDependency>()
				.StartUsingMethod(x => x.Start)
		);

		Assert.Equal(1, HasThrowingPropertyDependency.InstancesCreated);
		Assert.Equal(1, HasThrowingPropertyDependency.InstancesStarted);
	}

	[Fact]
	public void Starts_component_without_start_method()
	{
		ClassWithInstanceCount.InstancesCount = 0;
		_kernel.AddFacility<StartableFacility>(f => f.DeferredTryStart());
		_kernel.Register(Component.For<ClassWithInstanceCount>().Start());
		Assert.Equal(1, ClassWithInstanceCount.InstancesCount);
	}

	[Fact]
	public void Starts_component_without_start_method_AllTypes()
	{
		ClassWithInstanceCount.InstancesCount = 0;
		_kernel.AddFacility<StartableFacility>(f => f.DeferredTryStart());
		_kernel.Register(Classes.FromAssembly(_currentAssembly)
			.Where(t => t == typeof(ClassWithInstanceCount))
			.Configure(c => c.Start()));
		Assert.Equal(1, ClassWithInstanceCount.InstancesCount);
	}

	[Fact]
	public void TestComponentWithNoInterface()
	{
		_kernel.ComponentCreated += OnNoInterfaceStartableComponentStarted;

		var compNode = new MutableConfiguration("component");
		compNode.Attributes["id"] = "b";
		compNode.Attributes["startable"] = "true";
		compNode.Attributes["startMethod"] = "Start";
		compNode.Attributes["stopMethod"] = "Stop";

		_kernel.ConfigurationStore.AddComponentConfiguration("b", compNode);

		_kernel.AddFacility<StartableFacility>();
		_kernel.Register(Component.For<NoInterfaceStartableComponent>().Named("b"));

		Assert.True(_startableCreatedBeforeResolved, "Component was not properly started");

		var component = _kernel.Resolve<NoInterfaceStartableComponent>("b");

		Assert.NotNull(component);
		Assert.True(component.Started);
		Assert.False(component.Stopped);

		_kernel.ReleaseComponent(component);
		Assert.True(component.Stopped);
	}

	[Fact]
	public void TestInterfaceBasedStartable()
	{
		_kernel.ComponentCreated += OnStartableComponentStarted;

		_kernel.AddFacility<StartableFacility>();

		_kernel.Register(Component.For(typeof(StartableComponent)).Named("a"));

		Assert.True(_startableCreatedBeforeResolved, "Component was not properly started");

		var component = _kernel.Resolve<StartableComponent>("a");

		Assert.NotNull(component);
		Assert.True(component.Started);
		Assert.False(component.Stopped);

		_kernel.ReleaseComponent(component);
		Assert.True(component.Stopped);
	}

	[Fact]
	public void TestStartableCallsStartOnlyOnceOnError()
	{
		StartableWithError.StartedCount = 0;
		_kernel.AddFacility<StartableFacility>();

		var ex =
			Assert.Throws<Exception>(() =>
				_kernel.Register(Component.For<StartableWithError>(),
					Component.For<ICommon>().ImplementedBy<CommonImpl1>()));

		// Every additional registration causes Start to be called again and again...
		Assert.Equal("This should go bonk", ex.Message);
		Assert.Equal(1, StartableWithError.StartedCount);
	}

	/// <summary>
	///     This test has one startable component dependent on another, and both are dependent on a third generic component - all are singletons. We need to make sure we only get one instance of each
	///     component created.
	/// </summary>
	[Fact]
	public void TestStartableChainWithGenerics()
	{
		_kernel.AddFacility<StartableFacility>();

		// Add parent. This has a dependency so won't be started yet.
		_kernel.Register(Component.For(typeof(StartableChainParent)).Named("chainparent"));

		Assert.Equal(0, StartableChainDependency.Startcount);
		Assert.Equal(0, StartableChainDependency.Createcount);

		// Add generic dependency. This is not startable so won't get created. 
		_kernel.Register(Component.For(typeof(StartableChainGeneric<>)).Named("chaingeneric"));

		Assert.Equal(0, StartableChainDependency.Startcount);
		Assert.Equal(0, StartableChainDependency.Createcount);

		// Add dependency. This will satisfy the dependency so everything will start.
		_kernel.Register(Component.For(typeof(StartableChainDependency)).Named("chaindependency"));

		Assert.Equal(1, StartableChainParent.Startcount);
		Assert.Equal(1, StartableChainParent.Createcount);
		Assert.Equal(1, StartableChainDependency.Startcount);
		Assert.Equal(1, StartableChainDependency.Createcount);
		Assert.Equal(1, StartableChainGeneric<string>.Createcount);
	}

	[Fact]
	public void TestStartableCustomDependencies()
	{
		_kernel.ComponentCreated += OnStartableComponentStarted;

		_kernel.AddFacility<StartableFacility>();
		_kernel.Register(
			Component.For<StartableComponentCustomDependencies>()
				.Named("a")
				.DependsOn(Property.ForKey("config").Eq(1))
		);
		Assert.True(_startableCreatedBeforeResolved, "Component was not properly started");

		var component = _kernel.Resolve<StartableComponentCustomDependencies>("a");

		Assert.NotNull(component);
		Assert.True(component.Started);
		Assert.False(component.Stopped);

		_kernel.ReleaseComponent(component);
		Assert.True(component.Stopped);
	}

	[Fact]
	public void TestStartableExplicitFakeDependencies()
	{
		_kernel.ComponentCreated += OnStartableComponentStarted;

		var dependsOnSomething = new DependencyModel(null, typeof(ICommon), false);

		_kernel.AddFacility<StartableFacility>();
		_kernel.Register(
			Component.For<StartableComponent>()
				.AddDescriptor(new AddDependency(dependsOnSomething))
		);

		Assert.False(_startableCreatedBeforeResolved, "Component should not have started");

		_kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>());

		Assert.True(_startableCreatedBeforeResolved, "Component was not properly started");
	}

	[Fact]
	public void TestStartableWithRegisteredCustomDependencies()
	{
		_kernel.ComponentCreated += OnStartableComponentStarted;

		_kernel.AddFacility<StartableFacility>();

		var dependencies = new Dictionary<string, object> { { "config", 1 } };
		_kernel.Register(Component.For<StartableComponentCustomDependencies>().DependsOn(dependencies));

		Assert.True(_startableCreatedBeforeResolved, "Component was not properly started");

		var component = _kernel.Resolve<StartableComponentCustomDependencies>();

		Assert.NotNull(component);
		Assert.True(component.Started);
		Assert.False(component.Stopped);

		_kernel.ReleaseComponent(component);
		Assert.True(component.Stopped);
	}

	[Fact]
	public void Works_when_Start_and_Stop_methods_have_overloads()
	{
		_kernel.AddFacility<StartableFacility>();
		_kernel.Register(Component.For<WithOverloads>()
			.StartUsingMethod("Start")
			.StopUsingMethod("Stop"));
		var c = _kernel.Resolve<WithOverloads>();
		Assert.True(c.StartCalled);
		_kernel.ReleaseComponent(c);
		Assert.True(c.StopCalled);
	}
}

[Transient]
public class WithOverloads
{
	public bool StartCalled { get; set; }
	public bool StopCalled { get; set; }

	public void Start()
	{
		StartCalled = true;
	}

	public void Start(int fake)
	{
	}

	public void Stop()
	{
		StopCalled = true;
	}

	public void Stop(string fake)
	{
	}
}

public class AddDependency : IComponentModelDescriptor
{
	private readonly DependencyModel _dependency;

	public AddDependency(DependencyModel dependency)
	{
		_dependency = dependency;
	}

	public void BuildComponentModel(IKernel kernel, ComponentModel model)
	{
		model.Dependencies.Add(_dependency);
	}

	public void ConfigureComponentModel(IKernel kernel, ComponentModel model)
	{
	}
}