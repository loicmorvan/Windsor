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

using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Facilities.TypedFactory;
using Castle.Windsor.Tests.Interceptors;
using Castle.Windsor.Windsor;

namespace Castle.Windsor.Tests;

public class ModelInterceptorsSelectorTestCase
{
    [Fact]
    public void CanAddInterceptor_DirectSelection()
    {
        var reporter = new CallReporter();

        var container = new WindsorContainer();
        container.Register(
            Component.For<CallReporter>().Instance(reporter),
            Component.For<WasCalledInterceptor>(),
            Component.For<IWatcher>()
                .ImplementedBy<BirdWatcher>()
                .Named("bird.watcher")
                .LifeStyle.Transient);

        var selector = new WatcherInterceptorSelector();
        container.Kernel.ProxyFactory.AddInterceptorSelector(selector);

        reporter.WasCalled = false;
        var watcher = container.Resolve<IWatcher>();
        watcher.OnSomethingInterestingToWatch += delegate { };
        Assert.False(reporter.WasCalled);

        selector.Interceptors = InterceptorKind.Dummy;

        reporter.WasCalled = false;
        watcher = container.Resolve<IWatcher>();
        watcher.OnSomethingInterestingToWatch += delegate { };
        Assert.True(reporter.WasCalled);
    }

    [Fact]
    public void InterceptorSelectors_Are_Cumulative()
    {
        var reporter = new CallReporter();

        var container = new WindsorContainer();
        container.Register(
            Component.For<CallReporter>().Instance(reporter),
            Component.For<CountingInterceptor>(),
            Component.For<WasCalledInterceptor>(),
            Component.For<IWatcher>().ImplementedBy<BirdWatcher>().Named("bird.watcher").LifeStyle.Transient);

        var selector = new WatcherInterceptorSelector { Interceptors = InterceptorKind.Dummy };
        container.Kernel.ProxyFactory.AddInterceptorSelector(selector);
        container.Kernel.ProxyFactory.AddInterceptorSelector(new AnotherInterceptorSelector());

        var watcher = container.Resolve<IWatcher>();
        watcher.OnSomethingInterestingToWatch += delegate { };
        Assert.True(reporter.WasCalled);
        Assert.True(reporter.WasCalled);
    }

    [Fact]
    public void TurnProxyOnAndOff_DirectSelection()
    {
        var reporter = new CallReporter();
        var container = new WindsorContainer();
        container.Register(Component.For<WasCalledInterceptor>()).Register(
            Component.For<CallReporter>().Instance(reporter),
            Component.For<IWatcher>()
                .ImplementedBy<BirdWatcher>()
                .Named("bird.watcher")
                .LifeStyle.Transient);
        var selector = new WatcherInterceptorSelector();
        container.Kernel.ProxyFactory.AddInterceptorSelector(selector);

        Assert.DoesNotContain("Proxy", container.Resolve<IWatcher>().GetType().Name);
        selector.Interceptors = InterceptorKind.Dummy;
        Assert.Contains("Proxy", container.Resolve<IWatcher>().GetType().Name);
    }

    [Fact]
    public void TurnProxyOnAndOff_SubDependency()
    {
        var reporter = new CallReporter();
        var container = new WindsorContainer();
        container.Register(Component.For<WasCalledInterceptor>()).Register(
            Component.For<CallReporter>().Instance(reporter),
            Component.For(typeof(IWatcher)).ImplementedBy<BirdWatcher>().Named("bird.watcher").LifeStyle.Is(
                LifestyleType.Transient)).Register(Component.For(typeof(Person)).LifeStyle.Is(LifestyleType.Transient));
        var selector = new WatcherInterceptorSelector();
        container.Kernel.ProxyFactory.AddInterceptorSelector(selector);

        Assert.DoesNotContain("Proxy", container.Resolve<Person>().Watcher.GetType().Name);
        Assert.DoesNotContain("Proxy", container.Resolve<Person>().GetType().Name);

        selector.Interceptors = InterceptorKind.Dummy;

        Assert.DoesNotContain("Proxy", container.Resolve<Person>().GetType().Name);
        Assert.Contains("Proxy", container.Resolve<Person>().Watcher.GetType().Name);
    }

    [Fact]
    public void Interceptor_selected_by_selector_gets_released_properly()
    {
        var counter = new DataRepository();
        var container = new WindsorContainer();
        container.Kernel.ProxyFactory.AddInterceptorSelector(
            new ByTypeInterceptorSelector(typeof(DisposableInterceptor)));
        container.Register(
            Component.For<DataRepository>().Instance(counter),
            Component.For<DisposableInterceptor>(),
            Component.For<A>().LifeStyle.Transient);

        var a = container.Resolve<A>();
        Assert.Equal(1, counter[".ctor"]);

        container.Release(a);
        Assert.Equal(1, counter["Dispose"]);
    }
}