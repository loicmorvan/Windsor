// Copyright 2004-2022 Castle Project - http://www.castleproject.org/
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
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Context;

namespace Castle.Windsor.Facilities.TypedFactory.Internal;

public class DelegateProxyFactory : IProxyFactoryExtension
{
    public object Generate(IProxyBuilder builder, ProxyGenerationOptions options, IInterceptor[] interceptors,
        ComponentModel model,
        CreationContext context)
    {
        var targetDelegateType = context.RequestedType;
        var type = GetProxyType(builder, targetDelegateType);
        var instance = GetProxyInstance(type, interceptors);
        var method = GetInvokeDelegate(instance, targetDelegateType);
        return method;
    }

    private static object GetInvokeDelegate(object instance, Type targetDelegateType)
    {
        return instance.GetType().GetMethod("Invoke").CreateDelegate(targetDelegateType, instance);
    }

    private static object GetProxyInstance(Type type, IInterceptor[] interceptors)
    {
        return type.CreateInstance<object>(null, interceptors);
    }

    private static Type GetProxyType(IProxyBuilder builder, Type targetDelegateType)
    {
        var options = new ProxyGenerationOptions();
        options.AddDelegateTypeMixin(targetDelegateType);
        return builder.CreateClassProxyType(typeof(object), null, options);
    }
}