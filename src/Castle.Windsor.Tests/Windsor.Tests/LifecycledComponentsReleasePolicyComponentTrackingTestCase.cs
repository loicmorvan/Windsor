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

using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Facilities.TypedFactory;
using Castle.Windsor.Windsor;

namespace Castle.Windsor.Tests.Windsor.Tests;

public class LifecycledComponentsReleasePolicyComponentTrackingTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Disposable_singleton_as_dependency_of_non_disposable_transient_is_decommissionsed_with_container()
	{
		var lifecycleCounter = new LifecycleCounter();

		using (var windsorContainer = new WindsorContainer())
		{
			windsorContainer.Register(
				Component.For<LifecycleCounter>().Instance(lifecycleCounter),
				Component.For<HasCtorDependency>().LifeStyle.Transient,
				Component.For<ISimpleService>().ImplementedBy<SimpleServiceDisposable>());

			windsorContainer.Resolve<HasCtorDependency>();
		}

		Assert.Equal(1, lifecycleCounter.InstancesDisposed);
	}

	[Fact]
	public void Non_disposable_transient_with_disposable_singleton_as_dependency_is_not_tracked()
	{
		Container.Register(
			Component.For<LifecycleCounter>(),
			Component.For<HasCtorDependency>().LifeStyle.Transient,
			Component.For<ISimpleService>().ImplementedBy<SimpleServiceDisposable>());

		var root = Container.Resolve<HasCtorDependency>();

		Assert.False(Kernel.ReleasePolicy.HasTrack(root));
	}
}