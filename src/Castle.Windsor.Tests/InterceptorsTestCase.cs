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
using System.Linq;
using System.Threading;
using Castle.Core.Configuration;
using Castle.DynamicProxy;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Handlers;
using Castle.Windsor.MicroKernel.Proxy;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Interceptors;
using Castle.Windsor.Tests.ProxyInfrastructure;
using Castle.Windsor.Tests.XmlFiles;
using Castle.Windsor.Windsor;
using Castle.Windsor.Windsor.Installer;

namespace Castle.Windsor.Tests;

public class InterceptorsTestCase : IDisposable
{
	private readonly ManualResetEvent _startEvent = new(false);
	private readonly ManualResetEvent _stopEvent = new(false);

	private IWindsorContainer _container;
	private CalculatorService _service;

	public InterceptorsTestCase()
	{
		_container = new WindsorContainer();
		_container.AddFacility<MyInterceptorGreedyFacility>();
	}

	public void Dispose()
	{
		_container.Dispose();
	}

	[Fact]
	public void InterfaceProxy()
	{
		_container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
		_container.Register(Component.For(typeof(ICalcService)).ImplementedBy(typeof(CalculatorService)).Named("key"));

		var service = _container.Resolve<ICalcService>("key");

		Assert.NotNull(service);
		Assert.Equal(5, service.Sum(2, 2));
	}

	[Fact]
	public void Interface_that_depends_on_service_it_is_intercepting()
	{
		_container.Register(Component.For<InterceptorThatCauseStackOverflow>(),
			Component.For<ICameraService>().ImplementedBy<CameraService>().Interceptors<InterceptorThatCauseStackOverflow>(),
			//because it has no interceptors, it is okay to resolve it...
			Component.For<ICameraService>().ImplementedBy<CameraService>().Named("okay to resolve"));

		_container.Resolve<ICameraService>();
	}

	[Fact]
	public void InterfaceProxyWithLifecycle()
	{
		_container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
		_container.Register(Component.For(typeof(ICalcService)).ImplementedBy(typeof(CalculatorServiceWithLifecycle))
			.Named("key"));

		var service = _container.Resolve<ICalcService>("key");

		Assert.NotNull(service);
		Assert.True(service.Initialized);
		Assert.Equal(5, service.Sum(2, 2));

		Assert.False(service.Disposed);

		_container.Release(service);

		Assert.True(service.Disposed);
	}

	[Fact]
	public void ClassProxy()
	{
		_container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
		_container.Register(Component.For(typeof(CalculatorService)).Named("key"));

		_service = _container.Resolve<CalculatorService>("key");

		Assert.NotNull(_service);
		Assert.Equal(5, _service.Sum(2, 2));
	}

	[Fact]
	public void Xml_validComponent_resolves_correctly()
	{
		_container.Install(XmlResource("interceptors.xml"));
		_service = _container.Resolve<CalculatorService>("ValidComponent");

		Assert.NotNull(_service);
		Assert.Equal(5, _service.Sum(2, 2));
	}

	[Fact]
	public void Xml_multiple_interceptors_resolves_correctly()
	{
		_container.Install(XmlResource("interceptorsMultiple.xml"));
		_service = _container.Resolve<CalculatorService>("component");

		Assert.NotNull(_service);
		Assert.Equal(10, _service.Sum(2, 2));
	}

	[Fact]
	public void Xml_Component_With_Non_Existing_Interceptor_throws()
	{
		_container.Install(XmlResource("interceptors.xml"));
		Assert.Throws<HandlerException>(() =>
			_container.Resolve<CalculatorService>("ComponentWithNonExistingInterceptor"));
	}

	[Fact]
	public void Xml_Component_With_Non_invalid_Interceptor_throws()
	{
		Assert.Throws<Exception>(() =>
			_container.Install(XmlResource("interceptorsInvalid.xml")));
	}

	[Fact]
	public void Xml_mixin()
	{
		_container.Install(XmlResource("mixins.xml"));
		_service = _container.Resolve<CalculatorService>("ValidComponent");

		Assert.IsType<ISimpleMixIn>(_service, false);

		((ISimpleMixIn)_service).DoSomething();
	}

	[Fact]
	public void Xml_additionalInterfaces()
	{
		_container.Install(XmlResource("additionalInterfaces.xml"));
		_service = _container.Resolve<CalculatorService>("ValidComponent");

		Assert.IsType<ISimpleMixIn>(_service, false);

		Assert.Throws<NotImplementedException>(() =>
			((ISimpleMixIn)_service).DoSomething());
	}

	[Fact]
	public void Xml_hook_and_selector()
	{
		ProxyAllHook.Instances = 0;
		SelectAllSelector.Calls = 0;
		SelectAllSelector.Instances = 0;
		_container.Install(XmlResource("interceptorsWithHookAndSelector.xml"));
		var model = _container.Kernel.GetHandler("ValidComponent").ComponentModel;
		var options = model.ObtainProxyOptions(false);

		Assert.NotNull(options);
		Assert.NotNull(options.Selector);
		Assert.NotNull(options.Hook);
		Assert.Equal(0, SelectAllSelector.Instances);
		Assert.Equal(0, ProxyAllHook.Instances);

		_service = _container.Resolve<CalculatorService>("ValidComponent");

		Assert.Equal(1, SelectAllSelector.Instances);
		Assert.Equal(0, SelectAllSelector.Calls);
		Assert.Equal(1, ProxyAllHook.Instances);

		_service.Sum(2, 2);

		Assert.Equal(1, SelectAllSelector.Calls);
	}

	[Fact]
	public void OnBehalfOfTest()
	{
		_container.Register(Component.For(typeof(InterceptorWithOnBehalf)).Named("interceptor"));
		_container.Register(Component.For(typeof(CalculatorService)).Named("key"));

		var service = _container.Resolve<CalculatorService>("key");

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
		_container.Register(Component.For(typeof(GenericInterceptor<>)));
		_container.Register(Component.For(typeof(CalculatorService)).Interceptors<GenericInterceptor<object>>());

		var service = _container.Resolve<CalculatorService>();

		Assert.NotNull(service);
		Assert.Equal(4, service.Sum(2, 2));
	}

	[Fact]
	public void ClosedGenericInterceptor()
	{
		_container.Register(Component.For(typeof(GenericInterceptor<object>)));
		_container.Register(Component.For(typeof(CalculatorService)).Interceptors<GenericInterceptor<object>>());

		var service = _container.Resolve<CalculatorService>();

		Assert.NotNull(service);
		Assert.Equal(4, service.Sum(2, 2));
	}

	[Fact]
	public void ClassProxyWithAttributes()
	{
		_container = new WindsorContainer(); // So we wont use the facilities

		_container.Register(Component.For<ResultModifierInterceptor>(),
			Component.For<CalculatorServiceWithAttributes>());

		var service = _container.Resolve<CalculatorServiceWithAttributes>();

		Assert.NotNull(service);
		Assert.Equal(5, service.Sum(2, 2));
	}

	[Fact]
	public void Multithreaded()
	{
		_container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
		_container.Register(Component.For(typeof(CalculatorService)).Named("key"));

		_service = _container.Resolve<CalculatorService>("key");

		const int threadCount = 10;

		var threads = new Thread[threadCount];

		for (var i = 0; i < threadCount; i++)
		{
			threads[i] = new Thread(ExecuteMethodUntilSignal);
			threads[i].Start();
		}

		_startEvent.Set();

		Thread.CurrentThread.Join(2000);

		_stopEvent.Set();
	}

	[Fact]
	public void AutomaticallyOmitTarget()
	{
		_container.Register(
			Component.For<ICalcService>()
				.Interceptors(InterceptorReference.ForType<ReturnDefaultInterceptor>()).Last,
			Component.For<ReturnDefaultInterceptor>()
		);

		var calcService = _container.Resolve<ICalcService>();
		Assert.Equal(0, calcService.Sum(1, 2));
	}

	private void ExecuteMethodUntilSignal()
	{
		_startEvent.WaitOne(int.MaxValue);

		while (!_stopEvent.WaitOne(1))
		{
			Assert.Equal(5, _service.Sum(2, 2));
			Assert.Equal(6, _service.Sum(3, 2));
			Assert.Equal(8, _service.Sum(3, 4));
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