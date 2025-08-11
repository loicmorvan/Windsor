// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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
using JetBrains.Annotations;

namespace Castle.Windsor.Tests.Facilities.TypedFactory;

public sealed class TypedFactoryDisposeOrderTestCase : AbstractContainerTestCase
{
    protected override void AfterContainerCreated()
    {
        Container.AddFacility<TypedFactoryFacility>();
    }

    [Fact]
    public void Typed_factories_are_not_disposed_before_their_dependents()
    {
        Container.Register(
            Component.For<Dependency>(),
            Component.For<Dependent>());

        Container.Resolve<Dependent>();
    }

    [UsedImplicitly]
    public sealed class Dependency : IDisposable
    {
        private bool _isDisposed;

        public void Dispose()
        {
            _isDisposed = true;
        }

        public void Use()
        {
            ObjectDisposedException.ThrowIf(_isDisposed, typeof(Dependency));
        }
    }

    [UsedImplicitly]
    private sealed class Dependent(Func<Dependency> factory) : IDisposable
    {
        public void Dispose()
        {
            using var needed = factory.Invoke();
            needed.Use();
        }
    }
}