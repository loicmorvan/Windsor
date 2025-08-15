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

using Castle.DynamicProxy;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.ModelBuilder.Descriptors;

namespace Castle.Windsor.MicroKernel.Registration.Interceptor;

public class InterceptorGroup<TS>(ComponentRegistration<TS> registration, InterceptorReference[] interceptors)
    : RegistrationGroup<TS>(registration)
    where TS : class
{
    public ComponentRegistration<TS> Anywhere
    {
        get
        {
            AddDescriptor(new InterceptorDescriptor(interceptors));
            return Registration;
        }
    }

    public ComponentRegistration<TS> First
    {
        get
        {
            AddDescriptor(new InterceptorDescriptor(interceptors, InterceptorDescriptor.Where.First));
            return Registration;
        }
    }

    public ComponentRegistration<TS> Last
    {
        get
        {
            AddDescriptor(new InterceptorDescriptor(interceptors, InterceptorDescriptor.Where.Last));
            return Registration;
        }
    }

    public ComponentRegistration<TS> AtIndex(int index)
    {
        AddDescriptor(new InterceptorDescriptor(interceptors, index));
        return Registration;
    }

    public InterceptorGroup<TS> SelectedWith(IInterceptorSelector selector)
    {
        Registration.SelectInterceptorsWith(selector);
        return this;
    }
}