// Copyright 2018 Castle Project - http://www.castleproject.org/
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

using Castle.Windsor.Facilities.TypedFactory;
using Castle.Windsor.MicroKernel.Registration;

namespace Castle.Windsor.Tests.Facilities.TypedFactory;

public sealed class BurdenAddedToUnrelatedObjectTestCase : AbstractContainerTestCase
{
    protected override void AfterContainerCreated()
    {
        Container.AddFacility<TypedFactoryFacility>();
    }

    [Fact]
    public void Object_resolved_from_factory_is_not_added_as_burden_of_object_being_created()
    {
        Container.Register(
            Component.For(typeof(IFactory<>)).AsFactory(),
            Component.For<Foo>().LifeStyle.Transient,
            Component.For<LongLivedService>().LifeStyle.Singleton,
            Component.For<ShortLivedViewModel>().LifeStyle.Transient);

        var longLivedService = Container.Resolve<LongLivedService>();
        var shortLivedViewModel = Container.Resolve<ShortLivedViewModel>();

        var prematureDisposalHandler = new EventHandler((_, _) =>
            Assert.Fail("Long-lived service’s connection was disposed when short-lived view model was released."));

        longLivedService.SqlConnection.Disposed += prematureDisposalHandler;
        Container.Release(shortLivedViewModel);
        longLivedService.SqlConnection.Disposed -= prematureDisposalHandler;

        Container.Release(longLivedService);
    }

    public sealed class Foo : IDisposable
    {
        public void Dispose()
        {
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Disposed;
    }

    public interface IFactory<out T>
    {
        T Resolve();
    }

    public sealed class LongLivedService(IFactory<Foo> fooFactory)
    {
        private IFactory<Foo> FooFactory { get; } = fooFactory;

        public Foo SqlConnection { get; private set; }

        public void StartSomething()
        {
            SqlConnection = FooFactory.Resolve();
        }
    }

    public sealed class ShortLivedViewModel
    {
        public ShortLivedViewModel(IFactory<Foo> fooFactory, LongLivedService longLivedService)
        {
            longLivedService.StartSomething();
        }
    }
}