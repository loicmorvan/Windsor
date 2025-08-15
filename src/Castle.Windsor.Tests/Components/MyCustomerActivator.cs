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

using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.ComponentActivator;
using Castle.Windsor.MicroKernel.Context;
using Castle.Windsor.Tests.ClassComponents;

namespace Castle.Windsor.Tests.Components;

/// <summary>Summary description for MyCustomerActivator.</summary>
public class MyCustomerActivator(
    ComponentModel model,
    IKernelInternal kernel,
    ComponentInstanceDelegate onCreation,
    ComponentInstanceDelegate onDestruction)
    : AbstractComponentActivator(model, kernel, onCreation, onDestruction)
{
    protected override object InternalCreate(CreationContext context)
    {
        var customer = new CustomerImpl
        {
            Name = "James Bond"
        };
        return customer;
    }

    protected override void InternalDestroy(object instance)
    {
    }
}