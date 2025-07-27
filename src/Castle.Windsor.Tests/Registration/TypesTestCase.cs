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

using System.Reflection;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Interceptors;

namespace Castle.Windsor.Tests.Registration;

public class TypesTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Based_on_interface_types_registered()
	{
		Container.Register(Types.FromAssembly(GetCurrentAssembly())
			.BasedOn<ICommon>()
		);

		var handlers = Kernel.GetHandlers(typeof(ICommon));
		Assert.Single(handlers);

		handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
		Assert.True(handlers.Length > 1);
	}

	[Fact]
	public void Interface_registered_with_no_implementation_with_interceptor_can_be_used()
	{
		Container.Register(
			Component.For<ReturnDefaultInterceptor>(),
			Types.FromAssembly(GetCurrentAssembly())
				.BasedOn<ISimpleService>()
				.If(t => t.GetTypeInfo().IsInterface)
				.Configure(t => t.Interceptors<ReturnDefaultInterceptor>())
		);

		var common = Container.Resolve<ISimpleService>();
		common.Operation();
	}
}