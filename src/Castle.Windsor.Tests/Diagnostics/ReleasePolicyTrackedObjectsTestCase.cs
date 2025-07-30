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

using System.Collections;
using System.Diagnostics;
using Castle.Windsor.Facilities.TypedFactory;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Releasers;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.ContainerExtensions;
using Castle.Windsor.Tests.Facilities.TypedFactory;
using Castle.Windsor.Windsor.Diagnostics;
using Castle.Windsor.Windsor.Diagnostics.DebuggerViews;
using Castle.Windsor.Windsor.Diagnostics.Extensions;

namespace Castle.Windsor.Tests.Diagnostics;

public class ReleasePolicyTrackedObjectsTestCase : AbstractContainerTestCase
{
    private DebuggerViewItem GetTrackedObjects()
    {
        var subSystem = Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey) as IContainerDebuggerExtensionHost;
        Debug.Assert(subSystem != null);
        return subSystem.SelectMany(e => e.Attach()).SingleOrDefault(i => i.Name == ReleasePolicyTrackedObjects.Name);
    }

    private void Register()
    {
        Container.Register(
            Component.For<LifecycleCounter>(),
            Component.For<DisposableFoo>().LifeStyle.Transient);
    }

    [Fact]
    public void List_tracked_alive_instances()
    {
        Register();
        Container.Resolve<DisposableFoo>();
        Container.Resolve<DisposableFoo>();

        var objects = GetTrackedObjects();
        var values = (DebuggerViewItem[])objects.Value;
        Assert.Single(values);
        var viewItem = (MasterDetailsDebuggerViewItem)values.Single().Value;
        Assert.Equal(2, viewItem.Details.Length);
    }

    [Fact]
    public void List_tracked_alive_instances_in_subscopes()
    {
        Register();
        Container.AddFacility<TypedFactoryFacility>();
        Container.Resolve<DisposableFoo>();
        var fooFactory = Container.Resolve<Func<DisposableFoo>>();
        fooFactory.Invoke();

        var objects = GetTrackedObjects();
        var values = (DebuggerViewItem[])objects.Value;
        Assert.Equal(3, values.Length);
        var instances = values.SelectMany(v => ((MasterDetailsDebuggerViewItem)v.Value).Details).ToArray();
        Assert.Equal(4, instances.Length);
    }

    [Fact]
    public void List_tracked_alive_instances_only()
    {
        Register();
        var foo1 = Container.Resolve<DisposableFoo>();
        Container.Resolve<DisposableFoo>();
        Container.Release(foo1);

        var objects = GetTrackedObjects();
        var values = (DebuggerViewItem[])objects.Value;
        Assert.Single(values);
        var viewItem = (MasterDetailsDebuggerViewItem)values.Single().Value;
        Assert.Single(viewItem.Details);
    }

    [Fact]
    public void NoTrackingReleasePolicy_does_not_appear()
    {
#pragma warning disable 612,618
        Kernel.ReleasePolicy = new NoTrackingReleasePolicy();
#pragma warning restore 612,618
        Register();

        Container.Resolve<DisposableFoo>();
        var objects = GetTrackedObjects();
        Assert.Empty((ICollection)objects.Value);
    }

    [Fact]
    public void Present_even_when_no_objects_were_created()
    {
        var objects = GetTrackedObjects();
        Assert.NotNull(objects);
    }

    [Fact]
    public void custom_ReleasePolicy_is_not_shown_if_not_implement_the_interface()
    {
        Kernel.ReleasePolicy = new MyCustomReleasePolicy();
        Register();
        Container.Resolve<DisposableFoo>();
        var objects = GetTrackedObjects();
        Assert.Empty((ICollection)objects.Value);
    }
}