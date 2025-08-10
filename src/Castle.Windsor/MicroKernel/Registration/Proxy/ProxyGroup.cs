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
using Castle.Windsor.MicroKernel.ModelBuilder.Descriptors;
using JetBrains.Annotations;

namespace Castle.Windsor.MicroKernel.Registration.Proxy;

public class ProxyGroup<TS> : RegistrationGroup<TS>
    where TS : class
{
    public ProxyGroup(ComponentRegistration<TS> registration)
        : base(registration)
    {
    }

    [PublicAPI]
    public ComponentRegistration<TS> AsMarshalByRefClass =>
        AddAttributeDescriptor("marshalByRefProxy", bool.TrueString);

    public ComponentRegistration<TS> AdditionalInterfaces(params Type[] interfaces)
    {
        if (interfaces is { Length: > 0 })
        {
            AddDescriptor(new ProxyInterfacesDescriptor(interfaces));
        }

        return Registration;
    }

    public ComponentRegistration<TS> Hook(IProxyGenerationHook hook)
    {
        return Hook(r => r.Instance(hook));
    }

    public ComponentRegistration<TS> Hook(Action<ItemRegistration<IProxyGenerationHook>> hookRegistration)
    {
        var hook = new ItemRegistration<IProxyGenerationHook>();
        hookRegistration.Invoke(hook);

        AddDescriptor(new ProxyHookDescriptor(hook.Item));
        return Registration;
    }

    public ComponentRegistration<TS> MixIns(params object[] mixIns)
    {
        return MixIns(r => r.Objects(mixIns));
    }

    public ComponentRegistration<TS> MixIns(Action<MixinRegistration> mixinRegistration)
    {
        var mixins = new MixinRegistration();
        mixinRegistration.Invoke(mixins);

        AddDescriptor(new ProxyMixInsDescriptor(mixins));
        return Registration;
    }
}