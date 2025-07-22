// Copyright 2004-2014 Castle Project - http://www.castleproject.org/
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

using Castle.Windsor.Facilities.Startable;
using Castle.Windsor.MicroKernel.Handlers;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;

namespace Castle.Windsor.Tests.Facilities.Startable;

public class ManuallyTriggeredStartTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Can_manually_trigger_start()
	{
		var flag = new StartFlag();
		Components.Startable.Started = false;
		Container.AddFacility<StartableFacility>(f => f.DeferredStart(flag));
		Container.Register(Component.For<Components.Startable>(),
			Component.For<ICustomer>().ImplementedBy<CustomerImpl>());

		Assert.False(Components.Startable.Started);

		flag.Signal();

		Assert.True(Components.Startable.Started);
	}

	[Fact]
	public void Can_manually_trigger_start_only_once()
	{
		var flag = new StartFlag();
		Components.Startable.Started = false;
		Container.AddFacility<StartableFacility>(f => f.DeferredStart(flag));
		Container.Register(Component.For<Components.Startable>().LifestyleTransient(),
			Component.For<ICustomer>().ImplementedBy<CustomerImpl>());

		flag.Signal();
		Components.Startable.Started = false;
		flag.Signal();
		Assert.False(Components.Startable.Started);
	}

	[Fact]
	public void Can_manually_trigger_start_when_using_Install()
	{
		var flag = new StartFlag();
		Components.Startable.Started = false;
		Container.AddFacility<StartableFacility>(f => f.DeferredStart(flag));
		Container.Install(
			new ActionBasedInstaller(c => c.Register(Component.For<Components.Startable>(),
				Component.For<ICustomer>().ImplementedBy<CustomerImpl>()))
		);

		Assert.False(Components.Startable.Started);

		flag.Signal();

		Assert.True(Components.Startable.Started);
	}

	[Fact]
	public void Manually_triggered_start_throws_on_missing_dependencies()
	{
		var flag = new StartFlag();
		Components.Startable.Started = false;
		Container.AddFacility<StartableFacility>(f => f.DeferredStart(flag));
		Container.Register(Component.For<Components.Startable>());

		Assert.Throws<HandlerException>(() =>
			flag.Signal()
		);
	}
}