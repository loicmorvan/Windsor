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

using System.Reflection;
using Castle.DynamicProxy;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Interceptor;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Facilities;

namespace Castle.Windsor.Facilities.TypedFactory.Internal;

[Transient]
public class TypedFactoryInterceptor(IKernelInternal kernel, ITypedFactoryComponentSelector componentSelector)
    : IInterceptor, IOnBehalfAware, IDisposable
{
    private readonly IReleasePolicy _scope = kernel.ReleasePolicy.CreateSubPolicy();

    private bool _disposed;
    private IDictionary<MethodInfo, FactoryMethod> _methods;

    private ITypedFactoryComponentSelector ComponentSelector { get; } = componentSelector;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _scope.Dispose();
    }

    public void Intercept(IInvocation invocation)
    {
        // don't check whether the factory was already disposed: it may be a call to Dispose or
        // Release methods, which must remain functional after dispose as well
        if (TryGetMethod(invocation, out var method) == false)
        {
            throw new InvalidOperationException(
                $"Can't find information about factory method {invocation.Method}. This is most likely a bug. Please report it.");
        }

        switch (method)
        {
            case FactoryMethod.Resolve:
                Resolve(invocation);
                break;
            case FactoryMethod.Release:
                Release(invocation);
                break;
            case FactoryMethod.Dispose:
                Dispose();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void SetInterceptedComponentModel(ComponentModel target)
    {
        _methods =
            (IDictionary<MethodInfo, FactoryMethod>)target.ExtendedProperties[TypedFactoryFacility.FactoryMapCacheKey];
        if (_methods == null)
        {
            throw new ArgumentException(
                $"Component {target.Name} is not a typed factory. {GetType().Name} only works with typed factories.");
        }
    }

    private void Release(IInvocation invocation)
    {
        if (_disposed)
        {
            return;
        }

        foreach (var t in invocation.Arguments)
        {
            _scope.Release(t);
        }
    }

    private void Resolve(IInvocation invocation)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException("this", "The factory was disposed and can no longer be used.");
        }

        var component =
            ComponentSelector.SelectComponent(invocation.Method, invocation.TargetType, invocation.Arguments);
        if (component == null)
        {
            throw new FacilityException(
                $"Selector {ComponentSelector} didn't select any component for method {invocation.Method}. This usually signifies a bug in the selector.");
        }

        invocation.ReturnValue = component(kernel, _scope);
    }

    private bool TryGetMethod(IInvocation invocation, out FactoryMethod method)
    {
        if (_methods.TryGetValue(invocation.Method, out method))
        {
            return true;
        }

        return invocation.Method.IsGenericMethod &&
               _methods.TryGetValue(invocation.Method.GetGenericMethodDefinition(), out method);
    }
}