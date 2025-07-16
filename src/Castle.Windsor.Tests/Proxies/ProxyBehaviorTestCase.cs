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

namespace Castle.Proxies;

using System;
using System.Linq;

using Castle.Core;
using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Tests.ClassComponents;
using Castle.ProxyInfrastructure;
using Castle.Windsor.Installer;
using Castle.Windsor.Tests.Interceptors;
using Castle.XmlFiles;

using CastleTests;
using CastleTests.Components;

public class ProxyBehaviorTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Proxy_exposes_only_service_interfaces_from_configuration()
	{
		Container.Install(
			Configuration.FromXml(Xml.Embedded("proxyBehavior.xml")));
		var calcService = Container.Resolve<ICalcService>("default");
		Assert.NotNull(calcService);
		Assert.IsNotType<IDisposable>(calcService, exactMatch: false);
	}

	[Fact]
	public void ProxyGenarationHook_can_be_OnBehalfAware()
	{
		OnBehalfAwareProxyGenerationHook.target = null;
		Container.Register(Component.For<OnBehalfAwareProxyGenerationHook>().LifeStyle.Transient,
			Component.For<StandardInterceptor>().LifeStyle.Transient,
			Component.For<ISimpleService>()
				.ImplementedBy<SimpleService>()
				.LifeStyle.Transient
				.Interceptors<StandardInterceptor>()
				.Proxy.Hook(h => h.Service<OnBehalfAwareProxyGenerationHook>()));

		var service = Container.Resolve<ISimpleService>();

		Assert.True(ProxyServices.IsDynamicProxy(service.GetType()));
		Assert.NotNull(OnBehalfAwareProxyGenerationHook.target);
		Assert.Equal(typeof(ISimpleService), OnBehalfAwareProxyGenerationHook.target.Services.Single());
	}

	[Fact]
	public void OnBehalfAware_ProxyGenarationHook_works_on_dependencies()
	{
		OnBehalfAwareProxyGenerationHook.target = null;
		Container.Register(Component.For<OnBehalfAwareProxyGenerationHook>().LifeStyle.Transient,
			Component.For<StandardInterceptor>().LifeStyle.Transient,
			Component.For<UsesSimpleComponent1>().LifeStyle.Transient,
			Component.For<SimpleComponent1>().LifeStyle.Transient
				.Interceptors<StandardInterceptor>()
				.Proxy.Hook(h => h.Service<OnBehalfAwareProxyGenerationHook>()));

		var service = Container.Resolve<UsesSimpleComponent1>();

		Assert.True(ProxyServices.IsDynamicProxy(service.Dependency.GetType()));
		Assert.NotNull(OnBehalfAwareProxyGenerationHook.target);
		Assert.Equal(typeof(SimpleComponent1), OnBehalfAwareProxyGenerationHook.target.Services.Single());
	}

	[Fact]
	public void InterceptorSelector_can_be_OnBehalfAware()
	{
		OnBehalfAwareInterceptorSelector.target = null;
		Container.Register(Component.For<OnBehalfAwareInterceptorSelector>().LifeStyle.Transient,
			Component.For<StandardInterceptor>().LifeStyle.Transient,
			Component.For<ISimpleService>()
				.ImplementedBy<SimpleService>()
				.LifeStyle.Transient
				.Interceptors<StandardInterceptor>()
				.SelectInterceptorsWith(s => s.Service<OnBehalfAwareInterceptorSelector>()));

		var service = Container.Resolve<ISimpleService>();

		Assert.True(ProxyServices.IsDynamicProxy(service.GetType()));
		Assert.NotNull(OnBehalfAwareInterceptorSelector.target);
		Assert.Equal(typeof(ISimpleService), OnBehalfAwareInterceptorSelector.target.Services.Single());
	}

	[Fact]
	public void OnBehalfAware_InterceptorSelector_works_on_dependencies()
	{
		OnBehalfAwareInterceptorSelector.target = null;
		Container.Register(Component.For<OnBehalfAwareInterceptorSelector>().LifeStyle.Transient,
			Component.For<StandardInterceptor>().LifeStyle.Transient,
			Component.For<UsesSimpleComponent1>().LifeStyle.Transient,
			Component.For<SimpleComponent1>().LifeStyle.Transient
				.Interceptors<StandardInterceptor>()
				.SelectInterceptorsWith(s => s.Service<OnBehalfAwareInterceptorSelector>()));

		var service = Container.Resolve<UsesSimpleComponent1>();

		Assert.True(ProxyServices.IsDynamicProxy(service.Dependency.GetType()));
		Assert.NotNull(OnBehalfAwareInterceptorSelector.target);
		Assert.Equal(typeof(SimpleComponent1), OnBehalfAwareInterceptorSelector.target.Services.Single());
	}

	[Fact]
	public void Forwarded_type_proxy_implements_all_service_types_interface_services_only()
	{
		Container.Register(Component.For<StandardInterceptor>()
				.Named("a")
				.LifeStyle.Transient,
			Component.For<ICommon, ICommon2>()
				.ImplementedBy<TwoInterfacesImpl>()
				.Interceptors("a")
				.LifeStyle.Transient);

		var common = Container.Resolve<ICommon>();
		var common2 = Container.Resolve<ICommon2>();

		Assert.Same(common.GetType(), common2.GetType());
	}

	[Fact]
	public void Forwarded_type_proxy_implements_all_service_types_interface_and_class_services()
	{
		Container.Register(Component.For<StandardInterceptor>()
				.Named("a")
				.LifeStyle.Transient,
			Component.For<ICommon, ICommon2, TwoInterfacesImpl>()
				.ImplementedBy<TwoInterfacesImpl>()
				.Interceptors("a")
				.LifeStyle.Transient);

		var common = Container.Resolve<ICommon>();
		var common2 = Container.Resolve<ICommon2>();
		var impl = Container.Resolve<TwoInterfacesImpl>();

		Assert.Same(common.GetType(), common2.GetType());
		Assert.Same(common.GetType(), impl.GetType());
	}

	[Fact]
	public void Forwarded_type_proxy_implements_all_service_types_class_and_interface_services()
	{
		Container.Register(Component.For<StandardInterceptor>()
				.Named("a")
				.LifeStyle.Transient,
			Component.For<TwoInterfacesImpl, ICommon2, ICommon>()
				.ImplementedBy<TwoInterfacesImpl>()
				.Interceptors("a")
				.LifeStyle.Transient);

		var common = Container.Resolve<ICommon>();
		var common2 = Container.Resolve<ICommon2>();
		var impl = Container.Resolve<TwoInterfacesImpl>();

		Assert.Same(common.GetType(), common2.GetType());
		Assert.Same(common.GetType(), impl.GetType());
	}

	[Fact]
	public void Proxy_implements_only_service_interfaces()
	{
		Container.Register(Component.For<CountingInterceptor>()
				.Named("a"),
			Component.For<ICommon>()
				.ImplementedBy<TwoInterfacesImpl>()
				.Interceptors("a")
				.LifeStyle.Transient);

		var common = Container.Resolve<ICommon>();

		Assert.IsNotType<ICommon2>(common, exactMatch: false);
	}

	[Fact]
	public void Proxy_implements_only_service_interfaces_or_explicitly_added_interfaces()
	{
		Container.Register(Component.For<CountingInterceptor>()
				.Named("a"),
			Component.For<ICommon>()
				.ImplementedBy<TwoInterfacesImpl>()
				.Interceptors("a")
				.Proxy.AdditionalInterfaces(typeof(ICommon2))
				.LifeStyle.Transient);

		var common = Container.Resolve<ICommon>();

		Assert.IsType<ICommon2>(common, exactMatch: false);
	}

#if FEATURE_REMOTING
		[Fact]
		public void RequestMarshalByRefProxyWithAttribute()
		{
			Container.Register(Component.For<StandardInterceptor>(),
			                   Component.For<ICalcService>().ImplementedBy<CalculatorServiceWithMarshalByRefProxyBehavior>());

			var calcService = Container.Resolve<ICalcService>();

			Assert.IsNotType<CalculatorServiceWithMarshalByRefProxyBehavior>(calcService,
			                                                                       "Service proxy should not expose CalculatorServiceWithMarshalByRefProxyBehavior");
			Assert.IsType<MarshalByRefObject>(calcService, "Service proxy should expose MarshalByRefObject");
			Assert.IsNotType<IDisposable>(calcService, "Service proxy should expose the IDisposable interface");
		}

		[Fact]
		public void InternalInterfaceIgnoredByProxy()
		{
			Container.Install(
				Configuration.FromXml(Xml.Embedded("proxyBehavior.xml")));

			Container.Resolve<object>("hasInternalInterface");
		}
#endif
}