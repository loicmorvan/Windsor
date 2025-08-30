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

using Castle.Windsor.Facilities.TypedFactory;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;

namespace Castle.Windsor.Tests.Facilities.TypedFactory;

public class TypedFactoryOpenGenericFactoriesTestCase : AbstractContainerTestCase
{
    public TypedFactoryOpenGenericFactoriesTestCase()
    {
        Container.AddFacility<TypedFactoryFacility>();
    }

    [Fact]
    public void Can_use_open_generic_service_as_typed_factory()
    {
        Container.Register(Component.For(typeof(IGenericFactory<>)).AsFactory(),
            Component.For<A>(),
            Component.For<B>());

        var aFactory = Container.Resolve<IGenericFactory<A>>();
        var bFactory = Container.Resolve<IGenericFactory<B>>();

        Assert.NotNull(aFactory.Create());
        Assert.NotNull(bFactory.Create());
    }
}