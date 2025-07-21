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

namespace Castle.Windsor.Tests.Diagnostics;

using System.Linq;

using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Windsor.Diagnostics;

public class AllServicesDiagnosticTestCase : AbstractContainerTestCase
{
	private IAllServicesDiagnostic diagnostic;

	protected override void AfterContainerCreated()
	{
		var host = (IDiagnosticsHost)Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey);
		diagnostic = host.GetDiagnostic<IAllServicesDiagnostic>();
	}

	[Fact]
	public void Groups_components_by_exposed_service()
	{
		Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			Component.For<A>());

		var services = diagnostic.Inspect();
		Assert.Equal(2, services.Count);
		Assert.Equal(2, services[typeof(IEmptyService)].Count());
		Assert.Single(services[typeof(A)]);
	}

	[Fact]
	public void Open_generic_handlers_appear_once()
	{
		Container.Register(Component.For(typeof(GenericImpl1<>)));
		Container.Resolve<GenericImpl1<A>>();
		Container.Resolve<GenericImpl1<B>>();

		var services = diagnostic.Inspect();
		Assert.Equal(1, services.Count);
		Assert.True(services.Contains(typeof(GenericImpl1<>)));
	}

	[Fact]
	public void Works_for_multi_service_components()
	{
		Container.Register(Component.For<IEmptyService, EmptyServiceA>().ImplementedBy<EmptyServiceA>(),
			Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			Component.For<A>());

		var services = diagnostic.Inspect();
		Assert.Equal(3, services.Count);
	}
}