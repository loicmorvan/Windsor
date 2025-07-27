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

using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;

namespace Castle.Windsor.Tests;

public class GenericServiceStrategyTestCase : AbstractContainerTestCase
{
	[Fact]
	public void NOT_supports_returns_false_for_HasComponent()
	{
		Container.Register(Component.For(typeof(IGeneric<>))
			.ImplementedBy(typeof(GenericImpl1<>), new DelegatingServiceStrategy((_, _) => false)));

		Assert.False(Kernel.HasComponent(typeof(IGeneric<int>)));
	}

	[Fact]
	public void NOT_supports_returns_null_for_GetHandler()
	{
		Container.Register(Component.For(typeof(IGeneric<>))
			.ImplementedBy(typeof(GenericImpl1<>), new DelegatingServiceStrategy((_, _) => false)));

		Assert.Null(Kernel.GetHandler(typeof(IGeneric<int>)));
	}

	[Fact]
	public void NOT_supports_returns_zero_for_GetAssignableHandlers()
	{
		Container.Register(Component.For(typeof(IGeneric<>))
			.ImplementedBy(typeof(GenericImpl1<>), new DelegatingServiceStrategy((_, _) => false)));

		Assert.Empty(Kernel.GetAssignableHandlers(typeof(IGeneric<int>)));
	}

	[Fact]
	public void NOT_supports_returns_zero_for_GetHandlers()
	{
		Container.Register(Component.For(typeof(IGeneric<>))
			.ImplementedBy(typeof(GenericImpl1<>), new DelegatingServiceStrategy((_, _) => false)));

		Assert.Empty(Kernel.GetHandlers(typeof(IGeneric<int>)));
	}

	[Fact]
	public void NOT_supports_throws_ComponentNotFoundException_when_resolving()
	{
		Container.Register(Component.For(typeof(IGeneric<>))
			.ImplementedBy(typeof(GenericImpl1<>), new DelegatingServiceStrategy((_, _) => false)));

		Assert.Throws<ComponentNotFoundException>(() => Container.Resolve<IGeneric<int>>());
	}
}