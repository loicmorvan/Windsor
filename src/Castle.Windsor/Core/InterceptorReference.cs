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

using System.Diagnostics;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Context;
using Castle.Windsor.MicroKernel.Proxy;
using Castle.Windsor.MicroKernel.Resolvers;

namespace Castle.Windsor.Core;

/// <summary>Represents an reference to a Interceptor component.</summary>
[Serializable]
public class InterceptorReference : IReference<IInterceptor>, IEquatable<InterceptorReference>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string _referencedComponentName;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Type _referencedComponentType;

    /// <summary>Initializes a new instance of the <see cref="InterceptorReference" /> class.</summary>
    /// <param name="referencedComponentName">The component key.</param>
    public InterceptorReference(string referencedComponentName)
    {
        ArgumentNullException.ThrowIfNull(referencedComponentName);
        _referencedComponentName = referencedComponentName;
    }

    /// <summary>Initializes a new instance of the <see cref="InterceptorReference" /> class.</summary>
    /// <param name="componentType">
    ///     Type of the interceptor to use. This will reference the default component (ie. one with no
    ///     explicitly assigned name) implemented by given type.
    /// </param>
    public InterceptorReference(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        _referencedComponentName = ComponentName.DefaultNameFor(componentType);
        _referencedComponentType = componentType;
    }

    public bool Equals(InterceptorReference other)
    {
        if (other == null)
        {
            return false;
        }

        return Equals(_referencedComponentName, other._referencedComponentName);
    }

    void IReference<IInterceptor>.Attach(ComponentModel component)
    {
        component.Dependencies.Add(new ComponentDependencyModel(_referencedComponentName, ComponentType()));
    }

    void IReference<IInterceptor>.Detach(ComponentModel component)
    {
        throw new NotSupportedException();
    }

    IInterceptor IReference<IInterceptor>.Resolve(IKernel kernel, CreationContext context)
    {
        var handler = GetInterceptorHandler(kernel);
        if (handler == null)
        {
            var message = GetExceptionMessageOnHandlerNotFound(kernel);
            throw new DependencyResolverException(message.ToString());
        }

        if (handler.IsBeingResolvedInContext(context))
        {
            throw new DependencyResolverException(
                string.Format(
                    "Cycle detected - interceptor {0} wants to use itself as its interceptor. This usually signifies a bug in custom {1}",
                    handler.ComponentModel.Name, nameof(IModelInterceptorsSelector)));
        }

        var contextForInterceptor = RebuildContext(ComponentType(), context);
        return (IInterceptor)handler.Resolve(contextForInterceptor);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return Equals(obj as InterceptorReference);
    }

    public override int GetHashCode()
    {
        return _referencedComponentName.GetHashCode();
    }

    public override string ToString()
    {
        return _referencedComponentName;
    }

    private Type ComponentType()
    {
        return _referencedComponentType ?? typeof(IInterceptor);
    }

    private StringBuilder GetExceptionMessageOnHandlerNotFound(IKernel kernel)
    {
        var message = new StringBuilder($"The interceptor '{_referencedComponentName}' could not be resolved. ");
        // ok so the component is missing. Now - is it missing because it's not been registered or because the reference is by type and interceptor was registered with custom name?
        if (_referencedComponentType != null)
        {
            var typedHandler = kernel.GetHandler(_referencedComponentType);
            if (typedHandler != null)
            {
                message.Append(
                    $"Component '{typedHandler.ComponentModel.Name}' matching the type specified was found in the container. Did you mean to use it? If so, please specify the reference via name, or register the interceptor without specifying its name.");
                return message;
            }
        }

        message.Append("Did you forget to register it?");
        return message;
    }

    private IHandler GetInterceptorHandler(IKernel kernel)
    {
        if (_referencedComponentType != null)
        {
            //try old behavior first
            var handler = kernel.GetHandler(_referencedComponentType.FullName);
            if (handler != null)
            {
                return handler;
            }

            // new bahavior as a fallback
            return kernel.GetHandler(_referencedComponentType);
        }

        return kernel.GetHandler(_referencedComponentName);
    }

    private CreationContext RebuildContext(Type handlerType, CreationContext current)
    {
        if (handlerType.GetTypeInfo().ContainsGenericParameters)
        {
            return current;
        }

        return new CreationContext(handlerType, current, true);
    }

    /// <summary>Gets an <see cref="InterceptorReference" /> for the component key.</summary>
    /// <param name="key">The component key.</param>
    /// <returns>The <see cref="InterceptorReference" /></returns>
    public static InterceptorReference ForKey(string key)
    {
        return new InterceptorReference(key);
    }

    /// <summary>Gets an <see cref="InterceptorReference" /> for the service.</summary>
    /// <param name="service">The service.</param>
    /// <returns>The <see cref="InterceptorReference" /></returns>
    public static InterceptorReference ForType(Type service)
    {
        return new InterceptorReference(service);
    }

    /// <summary>Gets an <see cref="InterceptorReference" /> for the service.</summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The <see cref="InterceptorReference" /></returns>
    public static InterceptorReference ForType<T>()
    {
        return new InterceptorReference(typeof(T));
    }
}