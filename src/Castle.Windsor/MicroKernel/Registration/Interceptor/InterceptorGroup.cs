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

public class InterceptorGroup<TS> : RegistrationGroup<TS>
    where TS : class
{
    private readonly InterceptorReference[] _interceptors;

    public InterceptorGroup(ComponentRegistration<TS> registration, InterceptorReference[] interceptors)
        : base(registration)
    {
        _interceptors = interceptors;
    }

    public ComponentRegistration<TS> Anywhere
    {
        get
        {
            AddDescriptor(new InterceptorDescriptor(_interceptors));
            return Registration;
        }
    }

    public ComponentRegistration<TS> First
    {
        get
        {
            AddDescriptor(new InterceptorDescriptor(_interceptors, InterceptorDescriptor.Where.First));
            return Registration;
        }
    }

    public ComponentRegistration<TS> Last
    {
        get
        {
            AddDescriptor(new InterceptorDescriptor(_interceptors, InterceptorDescriptor.Where.Last));
            return Registration;
        }
    }

    public ComponentRegistration<TS> AtIndex(int index)
    {
        AddDescriptor(new InterceptorDescriptor(_interceptors, index));
        return Registration;
    }

    public InterceptorGroup<TS> SelectedWith(IInterceptorSelector selector)
    {
        Registration.SelectInterceptorsWith(selector);
        return this;
    }
}