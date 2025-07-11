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
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

public class CompositePatternTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Can_resolve_composite_based_solely_on_conventions()
	{
		Kernel.Resolver.AddSubResolver(new CollectionResolver(Kernel));
		Container.Register(
			Component.For<IEmptyService2>()
				.ImplementedBy<CompositeEmptyService2>()
				.LifeStyle.Transient,
			Classes.FromAssembly(GetCurrentAssembly())
				.BasedOn<IEmptyService2>()
				.WithService.Base()
				.Configure(c => c.LifestyleTransient()));

		var obj = Container.Resolve<IEmptyService2>();
		Assert.IsType<CompositeEmptyService2>(obj);
		Assert.Equal(5, ((CompositeEmptyService2)obj).Inner.Length);
	}
}