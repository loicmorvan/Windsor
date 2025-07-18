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

namespace Castle.Windsor.Tests;

using System;
using System.Linq;
using System.Threading;

using Castle.Core;
using Castle.Core.Configuration;
using Castle.Core.Internal;
using Castle.DynamicProxy;
using Castle.MicroKernel;
using Castle.MicroKernel.Handlers;
using Castle.MicroKernel.Proxy;
using Castle.MicroKernel.Registration;
using Castle.ProxyInfrastructure;
using Castle.Windsor.Installer;
using Castle.Windsor.Tests.Interceptors;
using Castle.XmlFiles;

using CastleTests.Components;

public class InterceptorsTestCase : IDisposable
{
	private readonly ManualResetEvent startEvent = new(false);
	private readonly ManualResetEvent stopEvent = new(false);

	private IWindsorContainer container;
	private CalculatorService service;

	public InterceptorsTestCase()
	{
		container = new WindsorContainer();
		container.AddFacility<MyInterceptorGreedyFacility>();
	}

	public void Dispose()
	{
		container.Dispose();
	}

	[Fact]
	public void InterfaceProxy()
	{
		container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
		container.Register(Component.For(typeof(ICalcService)).ImplementedBy(typeof(CalculatorService)).Named("key"));

		var service = container.Resolve<ICalcService>("key");

		Assert.NotNull(service);
		Assert.Equal(5, service.Sum(2, 2));
	}

	[Fact]
	public void Interface_that_depends_on_service_it_is_intercepting()
	{
		container.Register(Component.For<InterceptorThatCauseStackOverflow>(),
			Component.For<ICameraService>().ImplementedBy<CameraService>().Interceptors<InterceptorThatCauseStackOverflow>(),
			//because it has no interceptors, it is okay to resolve it...
			Component.For<ICameraService>().ImplementedBy<CameraService>().Named("okay to resolve"));

		container.Resolve<ICameraService>();
	}

	[Fact]
	public void InterfaceProxyWithLifecycle()
	{
		container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
		container.Register(Component.For(typeof(ICalcService)).ImplementedBy(typeof(CalculatorServiceWithLifecycle)).Named("key"));

		var service = container.Resolve<ICalcService>("key");

		Assert.NotNull(service);
		Assert.True(service.Initialized);
		Assert.Equal(5, service.Sum(2, 2));

		Assert.False(service.Disposed);

		container.Release(service);

		Assert.True(service.Disposed);
	}

	[Fact]
	public void ClassProxy()
	{
		container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
		container.Register(Component.For(typeof(CalculatorService)).Named("key"));

		service = container.Resolve<CalculatorService>("key");

		Assert.NotNull(service);
		Assert.Equal(5, service.Sum(2, 2));
	}

	[Fact]
	public void Xml_validComponent_resolves_correctly()
	{
		container.Install(XmlResource("interceptors.xml"));
		service = container.Resolve<CalculatorService>("ValidComponent");

		Assert.NotNull(service);
		Assert.Equal(5, service.Sum(2, 2));
	}

	[Fact]
	public void Xml_multiple_interceptors_resolves_correctly()
	{
		container.Install(XmlResource("interceptorsMultiple.xml"));
		service = container.Resolve<CalculatorService>("component");

		Assert.NotNull(service);
		Assert.Equal(10, service.Sum(2, 2));
	}

	[Fact]
	public void Xml_Component_With_Non_Existing_Interceptor_throws()
	{
		container.Install(XmlResource("interceptors.xml"));
		Assert.Throws<HandlerException>(() =>
			container.Resolve<CalculatorService>("ComponentWithNonExistingInterceptor"));
	}

	[Fact]
	public void Xml_Component_With_Non_invalid_Interceptor_throws()
	{
		Assert.Throws<Exception>(() =>
			container.Install(XmlResource("interceptorsInvalid.xml")));
	}

	[Fact]
	public void Xml_mixin()
	{
		container.Install(XmlResource("mixins.xml"));
		service = container.Resolve<CalculatorService>("ValidComponent");

		Assert.IsType<ISimpleMixIn>(service, false);

		((ISimpleMixIn)service).DoSomething();
	}

	[Fact]
	public void Xml_additionalInterfaces()
	{
		container.Install(XmlResource("additionalInterfaces.xml"));
		service = container.Resolve<CalculatorService>("ValidComponent");

		Assert.IsType<ISimpleMixIn>(service, false);

		Assert.Throws<NotImplementedException>(() =>
			((ISimpleMixIn)service).DoSomething());
	}

	[Fact]
	public void Xml_hook_and_selector()
	{
		ProxyAllHook.Instances = 0;
		SelectAllSelector.Calls = 0;
		SelectAllSelector.Instances = 0;
		container.Install(XmlResource("interceptorsWithHookAndSelector.xml"));
		var model = container.Kernel.GetHandler("ValidComponent").ComponentModel;
		var options = model.ObtainProxyOptions(false);

		Assert.NotNull(options);
		Assert.NotNull(options.Selector);
		Assert.NotNull(options.Hook);
		Assert.Equal(0, SelectAllSelector.Instances);
		Assert.Equal(0, ProxyAllHook.Instances);

		service = container.Resolve<CalculatorService>("ValidComponent");

		Assert.Equal(1, SelectAllSelector.Instances);
		Assert.Equal(0, SelectAllSelector.Calls);
		Assert.Equal(1, ProxyAllHook.Instances);

		service.Sum(2, 2);

		Assert.Equal(1, SelectAllSelector.Calls);
	}

	[Fact]
	public void OnBehalfOfTest()
	{
		container.Register(Component.For(typeof(InterceptorWithOnBehalf)).Named("interceptor"));
		container.Register(Component.For(typeof(CalculatorService)).Named("key"));

		var service = container.Resolve<CalculatorService>("key");

		Assert.NotNull(service);
		Assert.Equal(4, service.Sum(2, 2));
		Assert.NotNull(InterceptorWithOnBehalf.Model);
		Assert.Equal("key", InterceptorWithOnBehalf.Model.Name);
		Assert.Equal(typeof(CalculatorService),
			InterceptorWithOnBehalf.Model.Implementation);
	}

	[Fact]
	public void OpenGenericInterceporIsUsedAsClosedGenericInterceptor()
	{
		container.Register(Component.For(typeof(GenericInterceptor<>)));
		container.Register(Component.For(typeof(CalculatorService)).Interceptors<GenericInterceptor<object>>());

		var service = container.Resolve<CalculatorService>();

		Assert.NotNull(service);
		Assert.Equal(4, service.Sum(2, 2));
	}

	[Fact]
	public void ClosedGenericInterceptor()
	{
		container.Register(Component.For(typeof(GenericInterceptor<object>)));
		container.Register(Component.For(typeof(CalculatorService)).Interceptors<GenericInterceptor<object>>());

		var service = container.Resolve<CalculatorService>();

		Assert.NotNull(service);
		Assert.Equal(4, service.Sum(2, 2));
	}

	[Fact]
	public void ClassProxyWithAttributes()
	{
		container = new WindsorContainer(); // So we wont use the facilities

		container.Register(Component.For<ResultModifierInterceptor>(),
			Component.For<CalculatorServiceWithAttributes>());

		var service = container.Resolve<CalculatorServiceWithAttributes>();

		Assert.NotNull(service);
		Assert.Equal(5, service.Sum(2, 2));
	}

	[Fact]
	public void Multithreaded()
	{
		container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
		container.Register(Component.For(typeof(CalculatorService)).Named("key"));

		service = container.Resolve<CalculatorService>("key");

		const int threadCount = 10;

		var threads = new Thread[threadCount];

		for (var i = 0; i < threadCount; i++)
		{
			threads[i] = new Thread(ExecuteMethodUntilSignal);
			threads[i].Start();
		}

		startEvent.Set();

		Thread.CurrentThread.Join(2000);

		stopEvent.Set();
	}

	[Fact]
	public void AutomaticallyOmitTarget()
	{
		container.Register(
			Component.For<ICalcService>()
				.Interceptors(InterceptorReference.ForType<ReturnDefaultInterceptor>()).Last,
			Component.For<ReturnDefaultInterceptor>()
		);

		var calcService = container.Resolve<ICalcService>();
		Assert.Equal(0, calcService.Sum(1, 2));
	}

	private void ExecuteMethodUntilSignal()
	{
		startEvent.WaitOne(int.MaxValue);

		while (!stopEvent.WaitOne(1))
		{
			Assert.Equal(5, service.Sum(2, 2));
			Assert.Equal(6, service.Sum(3, 2));
			Assert.Equal(8, service.Sum(3, 4));
		}
	}

	private ConfigurationInstaller XmlResource(string fileName)
	{
		return Configuration.FromXml(Xml.Embedded(fileName));
	}
}

public class MyInterceptorGreedyFacility : IFacility
{
	public void Init(IKernel kernel, IConfiguration facilityConfig)
	{
		kernel.ComponentRegistered += OnComponentRegistered;
	}

	public void Terminate()
	{
	}

	private void OnComponentRegistered(string key, IHandler handler)
	{
		if (key == "key")
			handler.ComponentModel.Interceptors.Add(
				new InterceptorReference("interceptor"));
	}
}

public class MyInterceptorGreedyFacility2 : IFacility
{
	public void Init(IKernel kernel, IConfiguration facilityConfig)
	{
		kernel.ComponentRegistered += OnComponentRegistered;
	}

	public void Terminate()
	{
	}

	private void OnComponentRegistered(string key, IHandler handler)
	{
		if (handler.ComponentModel.Services.Any(s => s.Is<IInterceptor>())) return;

		handler.ComponentModel.Interceptors.Add(new InterceptorReference("interceptor"));
	}
}