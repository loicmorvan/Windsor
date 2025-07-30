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

using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Facilities.TypedFactory;
using Castle.Windsor.Tests.Interceptors;

namespace Castle.Windsor.Tests;

public class InterceptorLifecycleTestCase : AbstractContainerTestCase
{
    [Fact]
    public void Disposable_interceptor_gets_properly_released_when_the_component_gets_released()
    {
        var counter = new LifecycleCounter();
        Container.Register(
            Component.For<LifecycleCounter>().Instance(counter),
            Component.For<DisposableInterceptor>().LifestyleTransient(),
            Component.For<A>().LifestyleTransient().Interceptors<DisposableInterceptor>());

        var a = Container.Resolve<A>();

        Assert.Equal(1, counter[".ctor"]);

        Container.Release(a);

        Assert.Equal(1, counter["Dispose"]);
    }
}