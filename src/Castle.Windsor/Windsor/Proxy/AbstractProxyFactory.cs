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

using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Interceptor;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Context;
using Castle.Windsor.MicroKernel.Proxy;
using Castle.Windsor.MicroKernel.Resolvers;

namespace Castle.Windsor.Windsor.Proxy;

public abstract class AbstractProxyFactory : IProxyFactory
{
    private List<IModelInterceptorsSelector> _selectors;

    public abstract object Create(IKernel kernel, object instance, ComponentModel model, CreationContext context,
        params object[] constructorArguments);

    public abstract object Create(IProxyFactoryExtension customFactory, IKernel kernel, ComponentModel model,
        CreationContext context, params object[] constructorArguments);

    public abstract bool RequiresTargetInstance(IKernel kernel, ComponentModel model);

    public void AddInterceptorSelector(IModelInterceptorsSelector selector)
    {
        _selectors ??= [];
        _selectors.Add(selector);
    }

    public bool ShouldCreateProxy(ComponentModel model)
    {
        if (model.HasInterceptors)
        {
            return true;
        }

        var options = model.ObtainProxyOptions(false);
        if (options is { RequiresProxy: true })
        {
            return true;
        }

        return _selectors != null && _selectors.Any(s => s.HasInterceptors(model));
    }

    private InterceptorReference[] GetInterceptorsFor(ComponentModel model)
    {
        var interceptors = model.Interceptors.ToArray();
        if (_selectors != null)
        {
            interceptors = _selectors.Where(selector => selector.HasInterceptors(model)).Aggregate(interceptors,
                (current, selector) => selector.SelectInterceptors(model, current) ?? []);
        }

        return interceptors;
    }

    /// <summary>Obtains the interceptors associated with the component.</summary>
    /// <param name="kernel">The kernel instance</param>
    /// <param name="model">The component model</param>
    /// <param name="context">The creation context</param>
    /// <returns>interceptors array</returns>
    protected IInterceptor[] ObtainInterceptors(IKernel kernel, ComponentModel model, CreationContext context)
    {
        var interceptors = new List<IInterceptor>();
        foreach (IReference<IInterceptor> interceptorRef in GetInterceptorsFor(model))
        {
            try
            {
                var interceptor = interceptorRef.Resolve(kernel, context);
                SetOnBehalfAware(interceptor as IOnBehalfAware, model);
                interceptors.Add(interceptor);
            }
            catch (Exception e)
            {
                foreach (var interceptor in interceptors)
                {
                    kernel.ReleaseComponent(interceptor);
                }

                if (e is not InvalidCastException)
                {
                    throw;
                }

                var message =
                    $"An interceptor registered for {model.Name} doesn't implement the {nameof(IInterceptor)} interface";

                throw new DependencyResolverException(message);
            }
        }

        return interceptors.ToArray();
    }

    private static void SetOnBehalfAware(IOnBehalfAware onBehalfAware, ComponentModel target)
    {
        onBehalfAware?.SetInterceptedComponentModel(target);
    }
}