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

using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;

namespace Castle.Windsor.Tests.Registration;

public class AssignableHandlersTestCase : AbstractContainerTestCase
{
    [Fact]
    public void Ignores_generic_components_where_generic_constrants_are_violated()
    {
        Kernel.Register(Component.For<CustomerValidator>(),
            Component.For(typeof(CustomerChainValidator<>)));

        var handlers = Kernel.GetAssignableHandlers(typeof(IValidator<CustomerImpl>));

        Assert.Single(handlers);
    }
}