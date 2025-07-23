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
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Resolvers;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Interceptors;
using Castle.Windsor.Windsor;
using Castle.Windsor.Windsor.Diagnostics;

namespace Castle.Windsor.Tests.Diagnostics;

public class UsingContainerAsServiceLocatorDiagnosticTestCase : AbstractContainerTestCase
{
	private IUsingContainerAsServiceLocatorDiagnostic _diagnostic;

	protected override void AfterContainerCreated()
	{
		var host = Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey) as IDiagnosticsHost;
		_diagnostic = host.GetDiagnostic<IUsingContainerAsServiceLocatorDiagnostic>();
	}

	[Theory]
	[InlineData(typeof(IKernel))]
	[InlineData(typeof(IKernelInternal))]
	[InlineData(typeof(IKernelEvents))]
	[InlineData(typeof(IWindsorContainer))]
	[InlineData(typeof(DefaultKernel))]
	[InlineData(typeof(WindsorContainer))]
	public void Detects_ctor_dependency_on(Type type)
	{
		var generic = typeof(GenericWithCtor<>).MakeGenericType(type);
		Container.Register(Component.For(generic),
			Component.For<A>());

		var serviceLocators = _diagnostic.Inspect();
		Assert.Single(serviceLocators);
	}

	[Theory]
	[InlineData(typeof(IKernel))]
	[InlineData(typeof(IKernelInternal))]
	[InlineData(typeof(IKernelEvents))]
	[InlineData(typeof(IWindsorContainer))]
	[InlineData(typeof(DefaultKernel))]
	[InlineData(typeof(WindsorContainer))]
	public void Detects_property_dependency_on(Type type)
	{
		var generic = typeof(GenericWithProperty<>).MakeGenericType(type);
		Container.Register(Component.For(generic),
			Component.For<A>());

		var serviceLocators = _diagnostic.Inspect();
		Assert.Single(serviceLocators);
	}

	[Fact]
	public void Ignores_interceptors()
	{
		Container.Register(
			Component.For<DependsOnTViaCtorInterceptor<IKernel>>().Named("a"),
			Component.For<DependsOnTViaPropertyInterceptor<IKernel>>().Named("b"),
			Component.For<B>().Interceptors("a"),
			Component.For<A>().Interceptors("b"));

		var serviceLocators = _diagnostic.Inspect();
		Assert.Empty(serviceLocators);
	}

	[Fact]
	public void Ignores_lazy()
	{
		Container.Register(Component.For<ILazyComponentLoader>()
			.ImplementedBy<LazyOfTComponentLoader>());
		Container.Register(Component.For<B>(),
			Component.For<A>());

		Container.Resolve<Lazy<B>>(); // to trigger lazy registration of lazy

		var serviceLocators = _diagnostic.Inspect();
		Assert.Empty(serviceLocators);
	}

	[Fact]
	public void Successfully_handles_cases_with_no_SL_usages()
	{
		Container.Register(Component.For<B>(),
			Component.For<A>());

		var serviceLocators = _diagnostic.Inspect();
		Assert.Empty(serviceLocators);
	}
}