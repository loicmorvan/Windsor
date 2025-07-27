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

using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.TestInfrastructure;

namespace Castle.Windsor.Tests.Lifestyle;

public class ScopedLifetyleCustomScopesTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Can_use_custom_scope_accessor_with_scoped_lifestyle()
	{
		StaticScopeAccessor.ResetScope();
		Container.Register(Component.For<A>().LifestyleScoped<StaticScopeAccessor>());

		var a1 = Container.Resolve<A>();
		var a2 = Container.Resolve<A>();

		Assert.Same(a1, a2);
	}

	[Fact]
	public void Can_use_custom_scope_accessor_with_scoped_lifestyle_generic()
	{
		StaticScopeAccessor.ResetScope();
		Container.Register(Component.For<A>().LifestyleScoped<StaticScopeAccessor>());

		var a1 = Container.Resolve<A>();
		var a2 = Container.Resolve<A>();

		Assert.Same(a1, a2);
	}

	[Fact]
	public void Can_use_custom_scope_accessor_with_scoped_lifestyle_multiple()
	{
		StaticScopeAccessor.ResetScope();
		Container.Register(Classes.FromAssembly(GetCurrentAssembly())
			.Where(c => c.Is<A>())
			.LifestyleScoped<StaticScopeAccessor>());

		var a1 = Container.Resolve<A>();
		var a2 = Container.Resolve<A>();

		Assert.Same(a1, a2);
	}
}