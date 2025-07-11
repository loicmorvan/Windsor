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

using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Interceptors;

namespace Castle.Windsor.Tests;

public class DependencyGraphTestCase : AbstractContainerTestCase
{
	[Fact]
	public void CycleComponentGraphs()
	{
		Kernel.Register(Component.For<CycleA>().Named("a"));
		Kernel.Register(Component.For<CycleB>().Named("b"));

		var exception =
			Assert.Throws<CircularDependencyException>(() =>
				Kernel.Resolve<CycleA>("a"));
		var expectedMessage =
			string.Format(
				"Dependency cycle has been detected when trying to resolve component 'a'.{0}The resolution tree that resulted in the cycle is the following:{0}Component 'a' resolved as dependency of{0}	component 'b' resolved as dependency of{0}	component 'a' which is the root component being resolved.{0}",
				Environment.NewLine);
		Assert.Equal(expectedMessage, exception.Message);
	}

	[Fact]
	public void GraphInvalid()
	{
		Kernel.Register(Component.For<B>());
		Kernel.Register(Component.For<C>());

		var handlerB = Kernel.GetHandler(typeof(B));
		var handlerC = Kernel.GetHandler(typeof(C));

		Assert.Equal(HandlerState.WaitingDependency, handlerB.CurrentState);
		Assert.Equal(HandlerState.WaitingDependency, handlerC.CurrentState);
	}

	[Fact]
	public void GraphInvalidAndLateValidation()
	{
		Kernel.Register(Component.For<B>());
		Kernel.Register(Component.For<C>());

		var handlerB = Kernel.GetHandler(typeof(B));
		var handlerC = Kernel.GetHandler(typeof(C));

		Assert.Equal(HandlerState.WaitingDependency, handlerB.CurrentState);
		Assert.Equal(HandlerState.WaitingDependency, handlerC.CurrentState);

		Kernel.Register(Component.For<A>());

		Assert.Equal(HandlerState.Valid, handlerB.CurrentState);
		Assert.Equal(HandlerState.Valid, handlerC.CurrentState);
	}

	[Fact]
	public void Same_transient_interceptor_ctor_and_property_dependencies_no_cycle()
	{
		Kernel.Register(Component.For<CountingInterceptor>().LifeStyle.Transient,
			Component.For<APropCtor>().Interceptors<CountingInterceptor>().LifeStyle.Transient,
			Component.For<A>().Interceptors<CountingInterceptor>().LifeStyle.Transient,
			Component.For<A2>().Interceptors<CountingInterceptor>().LifeStyle.Transient);
		Kernel.Resolve<APropCtor>();
	}

	[Fact]
	public void Same_transient_interceptor_ctor_dependencies()
	{
		Kernel.Register(Component.For<CountingInterceptor>().LifeStyle.Transient,
			Component.For<A>().Interceptors<CountingInterceptor>().LifeStyle.Transient,
			Component.For<B>().Interceptors<CountingInterceptor>().LifeStyle.Transient,
			Component.For<C>().Interceptors<CountingInterceptor>().LifeStyle.Transient);
		Kernel.Resolve<C>();
	}

	[Fact]
	public void Same_transient_interceptor_property_dependencies_cycle()
	{
		Kernel.Register(Component.For<CountingInterceptor>().LifeStyle.Transient,
			Component.For<ACycleProp>().Interceptors<CountingInterceptor>().LifeStyle.Transient,
			Component.For<BCycleProp>().Interceptors<CountingInterceptor>().LifeStyle.Transient);
		Kernel.Resolve<ACycleProp>();
	}

	[Fact]
	public void Same_transient_interceptor_property_dependencies_no_cycle()
	{
		Kernel.Register(Component.For<CountingInterceptor>().LifeStyle.Transient,
			Component.For<AProp>().Interceptors<CountingInterceptor>().LifeStyle.Transient,
			Component.For<A>().Interceptors<CountingInterceptor>().LifeStyle.Transient);
		Kernel.Resolve<AProp>();
	}

	[Fact]
	public void ValidSituation()
	{
		Kernel.Register(Component.For<A>(),
			Component.For<B>(),
			Component.For<C>());

		Assert.NotNull(Kernel.Resolve<A>());
		Assert.NotNull(Kernel.Resolve<B>());
		Assert.NotNull(Kernel.Resolve<C>());
	}
}