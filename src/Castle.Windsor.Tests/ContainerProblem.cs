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

using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Windsor;

namespace Castle.Windsor.Tests;

public class ContainerProblem
{
    [Fact]
    public void CausesStackOverflow()
    {
        IWindsorContainer container = new WindsorContainer();

        container.Register(Component.For<IChild>().ImplementedBy<Child>().Named("child"));
        container.Register(Component.For<IParent>().ImplementedBy<Parent>().Named("parent"));

        // child or parent will cause a stack overflow...?

        // IChild child = (IChild)container["child"];
        // IParent parent = (IParent) container["parent"];
        container.Resolve<IParent>("parent");
    }
}