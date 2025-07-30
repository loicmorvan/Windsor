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

using Castle.DynamicProxy;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

public class KernelEventsComponentCreatedTestCase : AbstractContainerTestCase
{
    private readonly IList<KeyValuePair<ComponentModel, object>> _list =
        new List<KeyValuePair<ComponentModel, object>>();

    protected override void AfterContainerCreated()
    {
        _list.Clear();
        Container.Kernel.ComponentCreated += Kernel_ComponentCreated;
    }

    private void Kernel_ComponentCreated(ComponentModel model, object instance)
    {
        _list.Add(new KeyValuePair<ComponentModel, object>(model, instance));
    }


    [Fact]
    public void Event_raised_for_component_with_interceptor()
    {
        Container.Register(
            Component.For<IInterceptor>().ImplementedBy<StandardInterceptor>().LifestyleTransient(),
            Component.For<IService>().ImplementedBy<MyService>().Interceptors<StandardInterceptor>()
                .LifestyleTransient());

        var service = Container.Resolve<IService>();
        Assert.NotEmpty(_list);
        Assert.Contains(_list, t => t.Value == service);
    }
}