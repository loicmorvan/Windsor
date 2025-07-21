// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Facilities.AspNetCore.Tests;

using System;
using System.Collections.Generic;
using System.Linq;

using Castle.Core;
using Castle.Windsor.Facilities.AspNetCore.Tests.Fakes;
using Castle.Windsor.Facilities.AspNetCore.Tests.Framework;
using Castle.Windsor;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Windsor;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using TestContext = Castle.Windsor.Facilities.AspNetCore.Tests.Framework.TestContext;

public class WindsorRegistrationExtensionsTestCase : IDisposable
{
	private readonly TestContext testContext;

	public WindsorRegistrationExtensionsTestCase()
	{
		testContext = TestContextFactory.Get();
	}

	public void Dispose()
	{
		testContext?.Dispose();
	}

	[InlineData(typeof(ControllerWindsorOnly))]
	[InlineData(typeof(TagHelperWindsorOnly))]
	[InlineData(typeof(ViewComponentWindsorOnly))]
	[Theory]
	public void Should_resolve_WindsorOnly_Controllers_TagHelpers_and_ViewComponents_from_WindsorContainer(Type serviceType)
	{
		testContext.WindsorContainer.Resolve(serviceType);
	}

	[InlineData(typeof(ControllerServiceProviderOnly))]
	[InlineData(typeof(TagHelperServiceProviderOnly))]
	[InlineData(typeof(ViewComponentServiceProviderOnly))]
	[Theory]
	public void Should_resolve_ServiceProviderOnly_Controllers_TagHelpers_and_ViewComponents_from_ServiceProvider(Type serviceType)
	{
		testContext.ServiceProvider.GetRequiredService(serviceType);
	}


	[InlineData(typeof(ControllerCrossWired))]
	[InlineData(typeof(TagHelperCrossWired))]
	[InlineData(typeof(ViewComponentCrossWired))]
	[InlineData(typeof(ControllerServiceProviderOnly))]
	[InlineData(typeof(TagHelperServiceProviderOnly))]
	[InlineData(typeof(ViewComponentServiceProviderOnly))]
	[Theory]
	public void Should_resolve_ServiceProviderOnly_and_CrossWired_Controllers_TagHelpers_and_ViewComponents_from_WindsorContainer_and_ServiceProvider(Type serviceType)
	{
		testContext.WindsorContainer.Resolve(serviceType);
		testContext.ServiceProvider.GetRequiredService(serviceType);
	}

	[InlineData(typeof(CrossWiredScoped))]
	[InlineData(typeof(CrossWiredSingleton))]
	[Theory]
	public void Should_resolve_CrossWired_Singleton_and_Scoped_as_same_instance_from_WindsorContainer_and_ServiceProvider(Type serviceType)
	{
		var instanceA = testContext.WindsorContainer.Resolve(serviceType);
		var instanceB = testContext.ServiceProvider.GetRequiredService(serviceType);

		Assert.Equal(instanceB, instanceA);
	}

	[InlineData(typeof(CrossWiredTransient))]
	[Theory]
	public void Should_resolve_CrossWired_Transient_as_different_instances_from_WindsorContainer_and_ServiceProvider(Type serviceType)
	{
		var instanceA = testContext.WindsorContainer.Resolve(serviceType);
		var instanceB = testContext.ServiceProvider.GetRequiredService(serviceType);

		Assert.NotEqual(instanceB, instanceA);
	}

	[InlineData(typeof(CrossWiredSingletonDisposable))]
	[InlineData(typeof(WindsorOnlySingletonDisposable))]
	[Theory]
	public void Should_not_Dispose_CrossWired_or_WindsorOnly_Singleton_disposables_when_Disposing_Windsor_Scope(Type serviceType)
	{
		var singleton = (IDisposableObservable)testContext.WindsorContainer.Resolve(serviceType);
		testContext.DisposeWindsorScope();

		Assert.False(singleton.Disposed);
		Assert.Equal(0, singleton.DisposedCount);
	}

	[InlineData(typeof(WindsorOnlySingletonDisposable))]
	[Theory]
	public void Should_Dispose_WindsorOnly_Singleton_disposables_only_when_Disposing_WindsorContainer(Type serviceType)
	{
		var singleton = (IDisposableObservable)testContext.WindsorContainer.Resolve(serviceType);
		testContext.DisposeWindsorContainer();

		Assert.True(singleton.Disposed);
		Assert.Equal(1, singleton.DisposedCount);
	}

	[InlineData(typeof(CrossWiredSingletonDisposable))]
	[Theory]
	public void Should_not_Dispose_CrossWired_Singleton_disposables_when_Disposing_WindsorContainer_because_it_is_tracked_by_the_ServiceProvider(Type serviceType)
	{
		var singleton = (IDisposableObservable)testContext.WindsorContainer.Resolve(serviceType);
		testContext.DisposeWindsorContainer();

		Assert.False(singleton.Disposed);
		Assert.Equal(0, singleton.DisposedCount);
	}

	[InlineData(typeof(CrossWiredSingletonDisposable))]
	[InlineData(typeof(ServiceProviderOnlySingletonDisposable))]
	[Theory]
	public void Should_Dispose_CrossWired_and_ServiceProviderOnly_Singleton_disposables_when_Disposing_ServiceProvider(Type serviceType)
	{
		var singleton = (IDisposableObservable)testContext.ServiceProvider.GetRequiredService(serviceType);
		testContext.DisposeServiceProvider();

		Assert.True(singleton.Disposed);
		Assert.Equal(1, singleton.DisposedCount);
	}

	[InlineData(typeof(WindsorOnlyScopedDisposable))]
	[Theory]
	public void Should_Dispose_WindsorOnly_Scoped_disposables_when_Disposing_Windsor_Scope(Type serviceType)
	{
		var singleton = (IDisposableObservable)testContext.WindsorContainer.Resolve(serviceType);
		testContext.DisposeWindsorScope();

		Assert.True(singleton.Disposed);
		Assert.Equal(1, singleton.DisposedCount);
	}

	[InlineData(typeof(CrossWiredScopedDisposable))]
	[Theory]
	public void Should_not_Dispose_CrossWired_Scoped_disposables_when_Disposing_Windsor_Scope_because_it_is_tracked_by_the_ServiceProvider(Type serviceType)
	{
		var singleton = (IDisposableObservable)testContext.WindsorContainer.Resolve(serviceType);
		testContext.DisposeWindsorScope();

		Assert.False(singleton.Disposed);
		Assert.Equal(0, singleton.DisposedCount);
	}

	[InlineData(typeof(CrossWiredScopedDisposable))]
	[InlineData(typeof(CrossWiredTransientDisposable))]
	[Theory]
	public void Should_Dispose_CrossWired_Scoped_and_Transient_disposables_when_Disposing_ServiceProvider_Scope(Type serviceType)
	{
		IDisposableObservable scoped;
		using (var serviceProviderScope = testContext.ServiceProvider.CreateScope())
		{
			scoped = (IDisposableObservable)serviceProviderScope.ServiceProvider.GetRequiredService(serviceType);
		}

		Assert.True(scoped.Disposed);
		Assert.Equal(1, scoped.DisposedCount);
	}

	[InlineData(typeof(CrossWiredTransientDisposable))]
	[Theory]
	public void Should_not_Dispose_CrossWired_Transient_disposables_when_Disposing_Windsor_Scope_because_is_tracked_by_the_ServiceProvider(Type serviceType)
	{
		var singleton = (IDisposableObservable)testContext.WindsorContainer.Resolve(serviceType);
		testContext.DisposeWindsorScope();

		Assert.False(singleton.Disposed);
		Assert.Equal(0, singleton.DisposedCount);
	}

	[InlineData(typeof(CrossWiredSingletonDisposable))]
	[InlineData(typeof(ServiceProviderOnlySingletonDisposable))]
	[Theory]
	public void Should_not_Dispose_CrossWired_or_ServiceOnly_Singleton_disposables_when_Disposing_ServiceProviderScope(Type serviceType)
	{
		IDisposableObservable singleton;

		using (var serviceProviderScope = testContext.ServiceProvider.CreateScope())
		{
			singleton = (IDisposableObservable)serviceProviderScope.ServiceProvider.GetRequiredService(serviceType);
		}

		Assert.False(singleton.Disposed);
		Assert.Equal(0, singleton.DisposedCount);
	}

	[InlineData(typeof(CompositeTagHelper))]
	[InlineData(typeof(CompositeController))]
	[InlineData(typeof(CompositeViewComponent))]
	[Theory]
	public void Should_resolve_Composite_Singleton_from_WindsorContainer(Type compositeType)
	{
		testContext.WindsorContainer.Register(Component.For(compositeType).LifestyleSingleton());

		testContext.WindsorContainer.Resolve(compositeType);
	}

	[InlineData(typeof(CompositeTagHelper))]
	[InlineData(typeof(CompositeController))]
	[InlineData(typeof(CompositeViewComponent))]
	[Theory]
	public void Should_resolve_Composite_Scoped_from_WindsorContainer(Type compositeType)
	{
		testContext.WindsorContainer.Register(Component.For(compositeType).LifestyleScoped());

		testContext.WindsorContainer.Resolve(compositeType);
	}

	[InlineData(typeof(CompositeTagHelper))]
	[InlineData(typeof(CompositeController))]
	[InlineData(typeof(CompositeViewComponent))]
	[Theory]
	public void Should_resolve_Composite_Transient_from_WindsorContainer(Type compositeType)
	{
		testContext.WindsorContainer.Register(Component.For(compositeType).LifestyleTransient());

		testContext.WindsorContainer.Resolve(compositeType);
	}

	[InlineData(typeof(CompositeTagHelper))]
	[InlineData(typeof(CompositeController))]
	[InlineData(typeof(CompositeViewComponent))]
	[Theory]
	public void Should_resolve_Composite_Singleton_CrossWired_from_ServiceProvider(Type compositeType)
	{
		testContext.WindsorContainer.Register(Component.For(compositeType).CrossWired().LifestyleSingleton());

		using (var sp = testContext.ServiceCollection.BuildServiceProvider())
		{
			sp.GetRequiredService(compositeType);
		}
	}

	[InlineData(typeof(CompositeTagHelper))]
	[InlineData(typeof(CompositeController))]
	[InlineData(typeof(CompositeViewComponent))]
	[Theory]
	public void Should_resolve_Composite_Scoped_CrossWired_from_ServiceProvider(Type compositeType)
	{
		testContext.WindsorContainer.Register(Component.For(compositeType).CrossWired().LifestyleScoped());

		using (var sp = testContext.ServiceCollection.BuildServiceProvider())
		{
			sp.GetRequiredService(compositeType);
		}
	}

	[InlineData(typeof(CompositeTagHelper))]
	[InlineData(typeof(CompositeController))]
	[InlineData(typeof(CompositeViewComponent))]
	[Theory]
	public void Should_resolve_Composite_Transient_CrossWired_from_ServiceProvider(Type compositeType)
	{
		testContext.WindsorContainer.Register(Component.For(compositeType).CrossWired().LifestyleTransient());

		using (var sp = testContext.ServiceCollection.BuildServiceProvider())
		{
			sp.GetRequiredService(compositeType);
		}
	}

	[Fact]
	public void Should_resolve_Multiple_Transient_CrossWired_from_ServiceProvider()
	{
		testContext.WindsorContainer.Register(Types.FromAssemblyContaining<AuthorisationHandlerOne>()
			.BasedOn<IAuthorizationHandler>().WithServiceBase()
			.LifestyleTransient().Configure(c => c.CrossWired()));

		using (var sp = testContext.ServiceCollection.BuildServiceProvider())
		{
			var services = sp.GetServices<IAuthorizationHandler>();

			Assert.Equal(3, services.Count());
			Assert.Equal(
				new HashSet<Type>(services.Select(x => x.GetType())).Count,
				services.Select(s => s.GetType()).Count());
		}
	}

	[InlineData(LifestyleType.Bound)]
	[InlineData(LifestyleType.Custom)]
	[InlineData(LifestyleType.Pooled)]
	[InlineData(LifestyleType.Thread)]
	[Theory]
	//[InlineData(LifestyleType.Undefined)] // Already throws System.ArgumentOutOfRangeException: Undefined is not a valid lifestyle type 
	public void Should_throw_if_CrossWired_with_these_LifestyleTypes(LifestyleType unsupportedLifestyleType)
	{
		Assert.Throws<NotSupportedException>(() =>
		{
			var componentRegistration = Component.For<AnyComponent>().CrossWired();
			componentRegistration.LifeStyle.Is(unsupportedLifestyleType);
			if (unsupportedLifestyleType == LifestyleType.Custom) componentRegistration.LifestyleCustom<AnyComponentWithLifestyleManager>();
			testContext.WindsorContainer.Register(componentRegistration);
		});
	}

	[Fact]
	public void Should_throw_if_Facility_is_added_without_calling_CrossWiresInto_on_IWindsorContainer_AddFacility()
	{
		using (var container = new WindsorContainer())
		{
			Assert.Throws<InvalidOperationException>(() => { container.AddFacility<AspNetCoreFacility>(); });
		}
	}

	[Fact] // https://github.com/castleproject/Windsor/issues/411
	public void Should_resolve_IMiddleware_from_Windsor()
	{
		testContext.WindsorContainer.GetFacility<AspNetCoreFacility>().RegistersMiddlewareInto(testContext.ApplicationBuilder);

		testContext.WindsorContainer.Register(Component.For<AnyMiddleware>().LifestyleScoped().AsMiddleware());

		testContext.WindsorContainer.Resolve<AnyMiddleware>();
	}

	[Fact] // https://github.com/castleproject/Windsor/issues/411
	public void Should_resolve_IMiddleware_from_Windsor_with_custom_dependencies()
	{
		testContext.WindsorContainer.GetFacility<AspNetCoreFacility>().RegistersMiddlewareInto(testContext.ApplicationBuilder);

		testContext.WindsorContainer.Register(Component.For<AnyMiddleware>().DependsOn(Dependency.OnValue<AnyComponent>(new AnyComponent())).LifestyleScoped().AsMiddleware());

		testContext.WindsorContainer.Resolve<AnyMiddleware>();
	}
}