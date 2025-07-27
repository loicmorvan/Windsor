// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

using System.Linq;
using Castle.DynamicProxy;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Interceptors;

// ReSharper disable UnusedVariable

// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable NotAccessedVariable

namespace Castle.Windsor.Tests.Proxies;

public class MixinDependencyTestCase : AbstractContainerTestCase
{
    [Fact]
    public void Bound_mixin_reused_as_implicit_and_explicit_dependency_chain()
    {
        Container.Register(
            Component.For<CollectInvocationsInterceptor>().LifestyleTransient(),
            Component.For<IComponent>()
                .ImplementedBy<TrivialComponent>()
                .LifestyleBoundToNearest<IUse<IComponent>>(),
            Component.For<IUse<IComponent>, UseChain<IComponent>>()
                .ImplementedBy<UseChain<IComponent>>()
                .Proxy.MixIns(m => m.Component<TrivialComponent>())
                .Interceptors<CollectInvocationsInterceptor>(),
            Component.For<IUse<IComponent>>()
                .ImplementedBy<Use<IComponent>>()
                .Proxy.MixIns(m => m.Component<TrivialComponent>())
                .Interceptors<CollectInvocationsInterceptor>()
        );

        var outerProxy = (UseChain<IComponent>)Container.Resolve<IUse<IComponent>>();
        var innerProxy = outerProxy.Next;

        var id = ((IComponent)outerProxy).Id; // to trigger interception;
        var id2 = ((IComponent)innerProxy).Id; // to trigger interception;

        var outerMixin = ((CollectInvocationsInterceptor)((IProxyTargetAccessor)outerProxy).GetInterceptors()[0])
            .Invocations[0].InvocationTarget;
        var innerMixin = ((CollectInvocationsInterceptor)((IProxyTargetAccessor)innerProxy).GetInterceptors()[0])
            .Invocations[0].InvocationTarget;

        Assert.NotSame(innerMixin, outerMixin);
        Assert.Same(outerMixin, outerProxy.Dependency);
        Assert.Same(innerMixin, innerProxy.Dependency);
    }

    [Fact]
    public void Bound_mixin_reused_as_implicit_and_explicit_dependency_simple()
    {
        Container.Register(
            Component.For<CollectInvocationsInterceptor>(),
            Component.For<IComponent>()
                .ImplementedBy<TrivialComponent>()
                .LifestyleBoundTo<IUse<IComponent>>(),
            Component.For<IUse<IComponent>>()
                .ImplementedBy<Use<IComponent>>()
                .Proxy.MixIns(m => m.Component<TrivialComponent>())
                .Interceptors<CollectInvocationsInterceptor>());

        var proxy = Container.Resolve<IUse<IComponent>>();
        // ReSharper disable once UnusedVariable
        // ReSharper disable once SuspiciousTypeConversion.Global
        var id = ((IComponent)proxy).Id; // to trigger interception;
        var interceptor = Container.Resolve<CollectInvocationsInterceptor>();
        var mixin = interceptor.Invocations.Single().InvocationTarget;
        Assert.Same(mixin, proxy.Dependency);
    }
}