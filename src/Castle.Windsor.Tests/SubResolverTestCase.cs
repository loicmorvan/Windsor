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

using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

public class SubResolverTestCase
{
	[Fact]
	public void WillAskResolverWhenTryingToResolveDependencyAfterAnotherHandlerWasRegistered()
	{
		var resolver = new FooBarResolver();

		IKernel kernel = new DefaultKernel();
		kernel.Resolver.AddSubResolver(resolver);

		kernel.Register(Component.For<Foo>());
		var handler = kernel.GetHandler(typeof(Foo));

		Assert.Equal(HandlerState.WaitingDependency, handler.CurrentState);

		resolver.Result = 15;

		kernel.Register(Component.For<A>());

		Assert.Equal(HandlerState.Valid, handler.CurrentState);
	}

	[Fact]
	public void Sub_resolver_can_provide_null_as_the_value_to_use()
	{
		IKernel kernel = new DefaultKernel();
		kernel.Resolver.AddSubResolver(new NullResolver());

		kernel.Register(Component.For<ComponentWithDependencyNotInContainer>());

		Assert.Null(kernel.Resolve<ComponentWithDependencyNotInContainer>().DependencyNotInContainer);
	}

	public class Foo(int bar)
	{
		private int _bar = bar;
	}

	public class FooBarResolver : ISubDependencyResolver
	{
		public int? Result;

		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
			ComponentModel model, DependencyModel dependency)
		{
			return Result != null;
		}

		public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
			ComponentModel model, DependencyModel dependency)
		{
			return Result.Value;
		}
	}

	public sealed class ComponentWithDependencyNotInContainer(DependencyNotInContainer dependencyNotInContainer)
	{
		public DependencyNotInContainer DependencyNotInContainer { get; } = dependencyNotInContainer;
	}

	public sealed class DependencyNotInContainer;

	private sealed class NullResolver : ISubDependencyResolver
	{
		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
			ComponentModel model, DependencyModel dependency)
		{
			return true;
		}

		public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
			ComponentModel model, DependencyModel dependency)
		{
			return null;
		}
	}
}