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

using System;
using Castle.Facilities.AspNetCore.Resolvers;
using Castle.Facilities.AspNetCore.Tests.Fakes;
using Castle.Facilities.AspNetCore.Tests.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace Castle.Facilities.AspNetCore.Tests.Resolvers;

using TestContext = TestContext;

public class FrameworkDependencyResolverTestCase : IDisposable
{
	private readonly FrameworkDependencyResolver _frameworkDependencyResolver;

	private readonly TestContext _testContext;

	public FrameworkDependencyResolverTestCase()
	{
		_testContext = TestContextFactory.Get();
		_frameworkDependencyResolver = new FrameworkDependencyResolver(_testContext.ServiceCollection);
		_frameworkDependencyResolver.AcceptServiceProvider(_testContext.ServiceProvider);
	}

	public void Dispose()
	{
		_testContext.Dispose();
	}

	[Fact]
	public void Should_not_match_null()
	{
		Assert.False(_frameworkDependencyResolver.HasMatchingType(null));
	}

	[InlineData(typeof(ServiceProviderOnlyTransient))]
	[InlineData(typeof(ServiceProviderOnlyTransientGeneric<OpenOptions>))]
	[InlineData(typeof(ServiceProviderOnlyTransientGeneric<ClosedOptions>))]
	[InlineData(typeof(ServiceProviderOnlyTransientDisposable))]
	[InlineData(typeof(ServiceProviderOnlyScoped))]
	[InlineData(typeof(ServiceProviderOnlyScopedGeneric<OpenOptions>))]
	[InlineData(typeof(ServiceProviderOnlyScopedGeneric<ClosedOptions>))]
	[InlineData(typeof(ServiceProviderOnlyScopedDisposable))]
	[InlineData(typeof(ServiceProviderOnlySingleton))]
	[InlineData(typeof(ServiceProviderOnlySingletonGeneric<OpenOptions>))]
	[InlineData(typeof(ServiceProviderOnlySingletonGeneric<ClosedOptions>))]
	[InlineData(typeof(ServiceProviderOnlySingletonDisposable))]
	[InlineData(typeof(ControllerServiceProviderOnly))]
	[InlineData(typeof(TagHelperServiceProviderOnly))]
	[InlineData(typeof(ViewComponentServiceProviderOnly))]
	[Theory]
	public void Should_match_ServiceProvider_services(Type serviceType)
	{
		Assert.True(_frameworkDependencyResolver.HasMatchingType(serviceType));
	}

	[InlineData(typeof(CrossWiredTransient))]
	[InlineData(typeof(CrossWiredTransientGeneric<OpenOptions>))]
	[InlineData(typeof(CrossWiredTransientGeneric<ClosedOptions>))]
	[InlineData(typeof(CrossWiredTransientDisposable))]
	[InlineData(typeof(CrossWiredScoped))]
	[InlineData(typeof(CrossWiredScopedGeneric<OpenOptions>))]
	[InlineData(typeof(CrossWiredScopedGeneric<ClosedOptions>))]
	[InlineData(typeof(CrossWiredScopedDisposable))]
	[InlineData(typeof(CrossWiredSingleton))]
	[InlineData(typeof(CrossWiredSingletonGeneric<OpenOptions>))]
	[InlineData(typeof(CrossWiredSingletonGeneric<ClosedOptions>))]
	[InlineData(typeof(CrossWiredSingletonDisposable))]
	[InlineData(typeof(ControllerCrossWired))]
	[InlineData(typeof(TagHelperCrossWired))]
	[InlineData(typeof(ViewComponentCrossWired))]
	[Theory]
	public void Should_match_CrossWired_services(Type serviceType)
	{
		Assert.True(_frameworkDependencyResolver.HasMatchingType(serviceType));
	}

	[InlineData(typeof(WindsorOnlyTransient))]
	[InlineData(typeof(WindsorOnlyTransientGeneric<OpenOptions>))]
	[InlineData(typeof(WindsorOnlyTransientGeneric<ClosedOptions>))]
	[InlineData(typeof(WindsorOnlyTransientDisposable))]
	[InlineData(typeof(WindsorOnlyScoped))]
	[InlineData(typeof(WindsorOnlyScopedGeneric<OpenOptions>))]
	[InlineData(typeof(WindsorOnlyScopedGeneric<ClosedOptions>))]
	[InlineData(typeof(WindsorOnlyScopedDisposable))]
	[InlineData(typeof(WindsorOnlySingleton))]
	[InlineData(typeof(WindsorOnlySingletonGeneric<OpenOptions>))]
	[InlineData(typeof(WindsorOnlySingletonGeneric<ClosedOptions>))]
	[InlineData(typeof(WindsorOnlySingletonDisposable))]
	[InlineData(typeof(ControllerWindsorOnly))]
	[InlineData(typeof(TagHelperWindsorOnly))]
	[InlineData(typeof(ViewComponentWindsorOnly))]
	[Theory]
	public void Should_not_match_WindsorOnly_services(Type serviceType)
	{
		Assert.True(!_frameworkDependencyResolver.HasMatchingType(serviceType));
	}

	[InlineData(typeof(ServiceProviderOnlyTransient))]
	[InlineData(typeof(ServiceProviderOnlyTransientGeneric<OpenOptions>))]
	[InlineData(typeof(ServiceProviderOnlyTransientGeneric<ClosedOptions>))]
	[InlineData(typeof(ServiceProviderOnlyTransientDisposable))]
	[InlineData(typeof(ServiceProviderOnlyScoped))]
	[InlineData(typeof(ServiceProviderOnlyScopedGeneric<OpenOptions>))]
	[InlineData(typeof(ServiceProviderOnlyScopedGeneric<ClosedOptions>))]
	[InlineData(typeof(ServiceProviderOnlyScopedDisposable))]
	[InlineData(typeof(ServiceProviderOnlySingleton))]
	[InlineData(typeof(ServiceProviderOnlySingletonGeneric<OpenOptions>))]
	[InlineData(typeof(ServiceProviderOnlySingletonGeneric<ClosedOptions>))]
	[InlineData(typeof(ServiceProviderOnlySingletonDisposable))]
	[InlineData(typeof(ControllerServiceProviderOnly))]
	[InlineData(typeof(TagHelperServiceProviderOnly))]
	[InlineData(typeof(ViewComponentServiceProviderOnly))]
	[Theory]
	public void Should_resolve_all_ServiceProviderOnly_services_from_ServiceProvider(Type serviceType)
	{
		_testContext.ServiceProvider.GetRequiredService(serviceType);
	}

	[InlineData(typeof(CrossWiredTransient))]
	[InlineData(typeof(CrossWiredTransientGeneric<OpenOptions>))]
	[InlineData(typeof(CrossWiredTransientGeneric<ClosedOptions>))]
	[InlineData(typeof(CrossWiredTransientDisposable))]
	[InlineData(typeof(CrossWiredScoped))]
	[InlineData(typeof(CrossWiredScopedGeneric<OpenOptions>))]
	[InlineData(typeof(CrossWiredScopedGeneric<ClosedOptions>))]
	[InlineData(typeof(CrossWiredScopedDisposable))]
	[InlineData(typeof(CrossWiredSingleton))]
	[InlineData(typeof(CrossWiredSingletonGeneric<OpenOptions>))]
	[InlineData(typeof(CrossWiredSingletonGeneric<ClosedOptions>))]
	[InlineData(typeof(CrossWiredSingletonDisposable))]
	[InlineData(typeof(ControllerCrossWired))]
	[InlineData(typeof(TagHelperCrossWired))]
	[InlineData(typeof(ViewComponentCrossWired))]
	[Theory]
	public void Should_resolve_all_CrossWiredOnly_services_from_ServiceProvider(Type serviceType)
	{
		_testContext.ServiceProvider.GetRequiredService(serviceType);
	}

	[InlineData(typeof(ControllerCrossWired))]
	[InlineData(typeof(TagHelperCrossWired))]
	[InlineData(typeof(ViewComponentCrossWired))]
	[InlineData(typeof(ControllerWindsorOnly))]
	[InlineData(typeof(TagHelperWindsorOnly))]
	[InlineData(typeof(ViewComponentWindsorOnly))]
	[InlineData(typeof(ControllerServiceProviderOnly))]
	[InlineData(typeof(TagHelperServiceProviderOnly))]
	[InlineData(typeof(ViewComponentServiceProviderOnly))]
	[Theory]
	public void
		Should_resolve_ServiceProviderOnly_and_WindsorOnly_and_CrossWired_registered_Controllers_TagHelpers_and_ViewComponents_from_WindsorContainer(
			Type serviceType)
	{
		_testContext.WindsorContainer.Resolve(serviceType);
	}
}