// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Bugs.Ioc113;

using System.Collections.Generic;

using Castle.Facilities.Startable;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Tests.Bugs.Ioc113;

public class IoC_113_When_resolving_initializable_disposable_and_startable_component
{
	public IoC_113_When_resolving_initializable_disposable_and_startable_component()
	{
		kernel = new DefaultKernel();

		kernel.AddFacility<StartableFacility>();

		kernel.Register(
			Component.For<StartableDisposableAndInitializableComponent>()
				.LifeStyle.Transient
		);

		component = kernel.Resolve<StartableDisposableAndInitializableComponent>();
		component.DoSomething();
		kernel.ReleaseComponent(component);

		calledMethods = component.calledMethods;
	}

	private IKernel kernel;
	private StartableDisposableAndInitializableComponent component;
	private IList<SdiComponentMethods> calledMethods;

	[Fact]
	public void Should_call_DoSomething_between_start_and_stop()
	{
		Assert.Equal(SdiComponentMethods.DoSomething, calledMethods[2]);
	}

	[Fact]
	public void Should_call_all_methods_once()
	{
		Assert.Equal(5, component.calledMethods.Count);
	}

	[Fact]
	public void Should_call_initialize_before_start()
	{
		Assert.Equal(SdiComponentMethods.Initialize, calledMethods[0]);
		Assert.Equal(SdiComponentMethods.Start, calledMethods[1]);
	}

	[Fact]
	public void Should_call_stop_before_dispose()
	{
		Assert.Equal(SdiComponentMethods.Stop, calledMethods[3]);
		Assert.Equal(SdiComponentMethods.Dispose, calledMethods[4]);
	}
}