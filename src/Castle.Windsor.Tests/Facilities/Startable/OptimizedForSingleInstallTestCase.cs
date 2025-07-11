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

using Castle.Facilities.Startable;
using Castle.MicroKernel.Handlers;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;

namespace Castle.Windsor.Tests.Facilities.Startable;

public class OptimizedForSingleInstallTestCase : AbstractContainerTestCase
{
	protected override void AfterContainerCreated()
	{
		Container.AddFacility<StartableFacility>(f => f.DeferredStart());
		Components.Startable.Started = false;
	}

	[Fact]
	public void Appearing_missing_dependencies_dont_cause_component_to_be_started_before_the_end_of_Install()
	{
		Container.Install(new ActionBasedInstaller(c => c.Register(Component.For<Components.Startable>())),
			new ActionBasedInstaller(c =>
			{
				c.Register(Component.For<ICustomer>().ImplementedBy<CustomerImpl>());
				Assert.False(Components.Startable.Started);
			}));
		Assert.True(Components.Startable.Started);
	}

	[Fact]
	public void Facility_wont_try_to_start_anything_before_the_end_of_Install()
	{
		Container.Install(
			new ActionBasedInstaller(c => c.Register(Component.For<ICustomer>().ImplementedBy<CustomerImpl>())),
			new ActionBasedInstaller(c =>
			{
				c.Register(Component.For<Components.Startable>());
				Assert.False(Components.Startable.Started);
			}));
		Assert.True(Components.Startable.Started);
	}

	[Fact]
	public void Missing_dependencies_after_the_end_of_Install_cause_exception()
	{
		Assert.Throws<HandlerException>(() =>
			Container.Install(
				new ActionBasedInstaller(c => c.Register(Component.For<Components.Startable>()))));
	}

	[Fact]
	public void Missing_dependencies_after_the_end_of_Install_no_exception_when_tryStart_true()
	{
		var container = new WindsorContainer();
		container.AddFacility<StartableFacility>(f => f.DeferredTryStart());

		container.Install(new ActionBasedInstaller(c => c.Register(Component.For<Components.Startable>())));

		Assert.False(Components.Startable.Started);
	}

	[Fact]
	public void Missing_dependencies_after_the_end_of_Install_starts_after_adding_missing_dependency_after_Install()
	{
		var container = new WindsorContainer();
		container.AddFacility<StartableFacility>(f => f.DeferredTryStart());

		container.Install(new ActionBasedInstaller(c => c.Register(Component.For<Components.Startable>())));

		container.Register(Component.For<ICustomer>().ImplementedBy<CustomerImpl>());
		Assert.True(Components.Startable.Started);
	}
}