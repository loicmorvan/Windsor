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
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests.Lifecycle;

public class InitializableTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Initializable_components_are_not_tracked()
	{
		Container.Register(Component.For<ISimpleService>()
			.ImplementedBy<SimpleServiceInitializable>()
			.LifeStyle.Transient);

		ReferenceTracker
			.Track(() => Container.Resolve<ISimpleService>())
			.AssertNoLongerReferenced();
	}

	[Fact]
	public void Initializable_components_for_non_initializable_service_get_initialized_when_resolved()
	{
		Container.Register(Component.For<ISimpleService>()
			.ImplementedBy<SimpleServiceInitializable>()
			.LifeStyle.Transient);

		var server = (SimpleServiceInitializable)Container.Resolve<ISimpleService>();

		Assert.True(server.IsInitialized);
	}

	[Fact]
	public void Initializable_components_for_non_initializable_service_get_initialized_when_resolved_via_factoryMethod()
	{
		Container.Register(Component.For<ISimpleService>()
			.UsingFactoryMethod(() => new SimpleServiceInitializable())
			.LifeStyle.Transient);

		var server = (SimpleServiceInitializable)Container.Resolve<ISimpleService>();

		Assert.True(server.IsInitialized);
	}

	[Fact]
	public void Initializable_components_get_initialized_when_resolved()
	{
		Container.Register(Component.For<InitializableComponent>());

		var server = Container.Resolve<InitializableComponent>();

		Assert.True(server.IsInitialized);
	}
}