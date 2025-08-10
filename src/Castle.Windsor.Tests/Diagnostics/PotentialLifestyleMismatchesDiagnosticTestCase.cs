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

using System.Diagnostics;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Interceptors;
using Castle.Windsor.Windsor.Diagnostics;

namespace Castle.Windsor.Tests.Diagnostics;

public class PotentialLifestyleMismatchesDiagnosticTestCase : AbstractContainerTestCase
{
    private IPotentialLifestyleMismatchesDiagnostic _diagnostic;

    protected override void AfterContainerCreated()
    {
        var host = Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey) as IDiagnosticsHost;
        Debug.Assert(host != null);
        _diagnostic = host.GetDiagnostic<IPotentialLifestyleMismatchesDiagnostic>();
    }

    [Fact]
    public void Can_detect_singleton_depending_on_transient()
    {
        Container.Register(Component.For<B>().LifeStyle.Singleton,
            Component.For<A>().LifeStyle.Transient);

        var mismatches = _diagnostic.Inspect();
        Assert.Single(mismatches);
    }

    [Fact]
    public void Can_detect_singleton_depending_on_transient_directly_and_indirectly()
    {
        Container.Register(Component.For<Cba>().LifeStyle.Singleton,
            Component.For<B>().LifeStyle.Singleton,
            Component.For<A>().LifeStyle.Transient);

        var items = _diagnostic.Inspect();
        Assert.Equal(3, items.Length);
        var cbaMismatches = items.Where(i => i.First().ComponentModel.Services.Single() == typeof(Cba)).ToArray();
        Assert.Equal(2, cbaMismatches.Length);
    }

    [Fact]
    public void Can_detect_singleton_depending_on_transient_indirectly()
    {
        Container.Register(Component.For<C>().LifeStyle.Singleton,
            Component.For<B>().LifeStyle.Singleton,
            Component.For<A>().LifeStyle.Transient);

        var mismatches = _diagnostic.Inspect();
        Assert.Equal(2, mismatches.Length);
    }

    [Fact]
    public void Can_detect_singleton_depending_on_transient_indirectly_via_custom_lifestyle()
    {
        Container.Register(Component.For<C>().LifeStyle.Singleton,
            Component.For<B>().LifeStyle.Custom<CustomLifestyleManager>(),
            Component.For<A>().LifeStyle.Transient);

        var mismatches = _diagnostic.Inspect();
        Assert.Single(mismatches);
    }

    [Fact]
    public void Can_detect_singleton_depending_on_two_transients_directly_and_indirectly()
    {
        Container.Register(Component.For<Cba>().LifeStyle.Singleton,
            Component.For<B>().LifeStyle.Transient,
            Component.For<A>().LifeStyle.Transient);

        var items = _diagnostic.Inspect();
        Assert.Equal(2, items.Length);
        var cbaMismatches = items.Where(i => i.First().ComponentModel.Services.Single() == typeof(Cba)).ToArray();
        Assert.Equal(2, cbaMismatches.Length);
    }

    [Fact]
    public void Can_handle_dependency_cycles()
    {
        Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>(),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecoratorViaProperty>());

        var mismatches = _diagnostic.Inspect();
        Assert.Empty(mismatches);
    }

    [Fact]
    public void Decorators_dont_trigger_stack_overflow()
    {
        Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>(),
            Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
            Component.For<UsesIEmptyService>());
        var items = _diagnostic.Inspect();
        Assert.Empty(items);
    }

    [Fact]
    public void Does_not_crash_on_dependency_cycles()
    {
        Container.Register(Component.For<InterceptorThatCauseStackOverflow>().Named("interceptor"),
            Component.For<ICameraService>().ImplementedBy<CameraService>()
                .Interceptors<InterceptorThatCauseStackOverflow>(),
            Component.For<ICameraService>().ImplementedBy<CameraService>()
                .Named("ok to resolve - has no interceptors"));
        var items = _diagnostic.Inspect();
        Assert.Empty(items);
    }
}