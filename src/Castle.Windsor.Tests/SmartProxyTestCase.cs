// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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
using Castle.DynamicProxy;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Interceptors;
using Castle.Windsor.Windsor;

namespace Castle.Windsor.Tests;

#if FEATURE_REMOTING
	using System.Runtime.Remoting;
#endif



public class SmartProxyTestCase : IDisposable
{
	private readonly WindsorContainer _container;

	public SmartProxyTestCase()
	{
		_container = new WindsorContainer();

		_container.AddFacility<MyInterceptorGreedyFacility>();
	}

	public void Dispose()
	{
		_container.Dispose();
	}

	[Fact]
	public void ConcreteClassProxy()
	{
		_container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
		_container.Register(Component.For(typeof(CalculatorService)).Named("key"));

		var service = _container.Resolve<CalculatorService>("key");

		Assert.NotNull(service);
#if FEATURE_REMOTING
			Assert.False(RemotingServices.IsTransparentProxy(service));
#endif
		Assert.Equal(5, service.Sum(2, 2));
	}

	[Fact]
	public void InterfaceInheritance()
	{
		_container.Register(Component.For<StandardInterceptor>().Named("interceptor"));
		_container.Register(Component.For<ICameraService>().ImplementedBy<CameraService>());

		var service = _container.Resolve<ICameraService>();

		Assert.NotNull(service);
	}

	[Fact]
	public void InterfaceProxy()
	{
		_container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
		_container.Register(Component.For(typeof(ICalcService)).ImplementedBy(typeof(CalculatorService)).Named("key"));

		var service = _container.Resolve<ICalcService>("key");

		Assert.NotNull(service);
#if FEATURE_REMOTING
			Assert.False(RemotingServices.IsTransparentProxy(service));
#endif
		Assert.Equal(5, service.Sum(2, 2));
	}
}