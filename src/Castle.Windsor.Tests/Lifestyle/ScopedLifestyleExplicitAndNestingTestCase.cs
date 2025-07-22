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

using Castle.Windsor.MicroKernel.Lifestyle;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests.Lifestyle;

public class ScopedLifestyleExplicitAndNestingTestCase : AbstractContainerTestCase
{
	protected override void AfterContainerCreated()
	{
		Container.Register(Component.For<A>().LifestyleScoped());
	}

	[Fact]
	public void Inner_scope_should_not_cause_outer_one_to_drop_cache()
	{
		using (Container.BeginScope())
		{
			var before = Container.Resolve<A>();
			using (Container.BeginScope())
			{
				Container.Resolve<A>();
			}

			var after = Container.Resolve<A>();
			Assert.Same(before, after);
		}
	}

	[Fact]
	public void Inner_scope_should_not_cause_outer_one_to_prematurely_release_components()
	{
		Container.Register(Component.For<ADisposable>().LifestyleScoped());
		using (Container.BeginScope())
		{
			var outer = Container.Resolve<ADisposable>();
			using (Container.BeginScope())
			{
				Container.Resolve<ADisposable>();
			}

			Assert.False(outer.Disposed);
		}
	}
}