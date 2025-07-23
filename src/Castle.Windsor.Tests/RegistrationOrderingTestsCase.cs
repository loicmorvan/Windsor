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
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

public class RegistrationOrderingTestsCase : AbstractContainerTestCase
{
	[Fact]
	public void CtorSourceOrderDoesNotMatter()
	{
		Container.Register(Component.For<DDb>());

		Assert.NotNull(Container.Resolve<DDb>());
	}

	[Fact]
	public void LoadingInSequence()
	{
		Container.Register(Component.For<A>(),
			Component.For<B>(),
			Component.For<C>());

		Assert.NotNull(Container.Resolve<C>());
		Assert.NotNull(Container.Resolve<B>());
		Assert.NotNull(Container.Resolve<A>());
	}

	[Fact]
	public void LoadingOutOfSequence()
	{
		Container.Register(Component.For<C>(),
			Component.For<B>(),
			Component.For<A>());

		Assert.NotNull(Container.Resolve<C>());
		Assert.NotNull(Container.Resolve<B>());
		Assert.NotNull(Container.Resolve<A>());
	}

	[Fact]
	public void LoadingOutOfSequenceWithExtraLoad()
	{
		Container.Register(Component.For<C>(),
			Component.For<B>(),
			Component.For<A>(),
			Component.For<object>());

		Assert.NotNull(Container.Resolve<C>());
		Assert.NotNull(Container.Resolve<B>());
		Assert.NotNull(Container.Resolve<A>());
	}

	[Fact]
	public void LoadingPartiallyInSequence()
	{
		Container.Register(Component.For<B>(),
			Component.For<C>(),
			Component.For<A>());

		Assert.NotNull(Container.Resolve<C>());
		Assert.NotNull(Container.Resolve<B>());
		Assert.NotNull(Container.Resolve<A>());
	}
}