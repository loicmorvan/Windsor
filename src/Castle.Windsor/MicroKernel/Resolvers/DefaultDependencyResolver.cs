// Copyright 2004-2014 Castle Project - http://www.castleproject.org/
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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel.Context;
using Castle.Windsor.MicroKernel.Handlers;
using Castle.Windsor.MicroKernel.SubSystems.Conversion;
using JetBrains.Annotations;

namespace Castle.Windsor.MicroKernel.Resolvers;

/// <summary>
///     Default implementation for <see cref="IDependencyResolver" />. This implementation is quite simple, but still
///     should be useful for 99% of situations.
/// </summary>
[Serializable]
public class DefaultDependencyResolver(IKernelInternal kernel, DependencyDelegate dependencyResolvingDelegate)
    : IDependencyResolver
{
    private readonly List<ISubDependencyResolver> _subResolvers = [];
    private ITypeConverter _converter = kernel.GetConversionManager();
    private DependencyDelegate _dependencyResolvingDelegate = dependencyResolvingDelegate;
    private IKernelInternal _kernel = kernel;

    /// <summary>Registers a sub resolver instance</summary>
    /// <param name="subResolver">The subresolver instance</param>
    public void AddSubResolver(ISubDependencyResolver subResolver)
    {
        ArgumentNullException.ThrowIfNull(subResolver);

        _subResolvers.Add(subResolver);
    }

    /// <summary>Unregisters a sub resolver instance previously registered</summary>
    /// <param name="subResolver">The subresolver instance</param>
    public void RemoveSubResolver(ISubDependencyResolver subResolver)
    {
        _subResolvers.Remove(subResolver);
    }

    /// <summary>Returns true if the resolver is able to satisfy the specified dependency.</summary>
    /// <param name="context">Creation context, which is a resolver itself</param>
    /// <param name="contextHandlerResolver">Parent resolver</param>
    /// <param name="model">Model of the component that is requesting the dependency</param>
    /// <param name="dependency">The dependency model</param>
    /// <returns><c>true</c> if the dependency can be satisfied</returns>
    public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
        DependencyModel dependency)
    {
        // 1 - check for the dependency on CreationContext, if present
        if (CanResolveFromContext(context, contextHandlerResolver, model, dependency))
        {
            return true;
        }

        // 2 - check with the model's handler, if not the same as the parent resolver
        if (CanResolveFromHandler(context, contextHandlerResolver, model, dependency))
        {
            return true;
        }

        // 3 - check within parent resolver, if present
        if (CanResolveFromContextHandlerResolver(context, contextHandlerResolver, model, dependency))
        {
            return true;
        }

        // 4 - check within subresolvers
        return CanResolveFromSubResolvers(context, contextHandlerResolver, model, dependency) ||
               // 5 - normal flow, checking against the kernel
               CanResolveFromKernel(context, model, dependency);
    }

    /// <summary>
    ///     Try to resolve the dependency by checking the parameters in the model or checking the Kernel for the requested
    ///     service.
    /// </summary>
    /// <remarks>
    ///     The dependency resolver has the following precedence order:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>The dependency is checked within the <see cref="CreationContext" /></description>
    ///         </item>
    ///         <item>
    ///             <description>The dependency is checked within the <see cref="IHandler" /> instance for the component</description>
    ///         </item>
    ///         <item>
    ///             <description>The dependency is checked within the registered <see cref="ISubDependencyResolver" /> s</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Finally the resolver tries the normal flow which is using the configuration or other component
    ///                 to satisfy the dependency
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <param name="context">Creation context, which is a resolver itself</param>
    /// <param name="contextHandlerResolver">Parent resolver</param>
    /// <param name="model">Model of the component that is requesting the dependency</param>
    /// <param name="dependency">The dependency model</param>
    /// <returns>The dependency resolved value or null</returns>
    public object? Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
        DependencyModel dependency)
    {
        if (!TryResolveCore(context, contextHandlerResolver, model, dependency, out var value))
        {
            if (dependency.HasDefaultValue)
            {
                value = dependency.DefaultValue;
            }
            else if (!dependency.IsOptional)
            {
                var message =
                    $"Could not resolve non-optional dependency for '{model.Name}' ({(model.Implementation != null ? model.Implementation.FullName : "-unknown-")})." +
                    $" Parameter '{dependency.DependencyKey}' type '{dependency.TargetType?.FullName ?? "nothing?"}'";

                throw new DependencyResolverException(message);
            }
        }

        _dependencyResolvingDelegate(model, dependency, value);
        return value;
    }

    /// <summary>Initializes this instance with the specified dependency delegate.</summary>
    /// <param name="kernel">kernel</param>
    /// <param name="dependencyDelegate">The dependency delegate.</param>
    public void Initialize(IKernelInternal kernel, DependencyDelegate dependencyDelegate)
    {
        _kernel = kernel;
        _converter = kernel.GetConversionManager();
        _dependencyResolvingDelegate = dependencyDelegate;
    }

    [PublicAPI]
    protected virtual bool CanResolveFromKernel(CreationContext context, ComponentModel model,
        DependencyModel dependency)
    {
        if (dependency.ReferencedComponentName != null)
            // User wants to override
        {
            return HasComponentInValidState(dependency.ReferencedComponentName, dependency, context);
        }

        if (dependency.Parameter != null)
        {
            return true;
        }

        if (typeof(IKernel).IsAssignableFrom(dependency.TargetItemType))
        {
            return true;
        }

        return !dependency.TargetItemType.IsPrimitiveType() &&
               HasAnyComponentInValidState(dependency.TargetItemType, dependency, context);
    }

    /// <summary>This method rebuild the context for the parameter type. Naive implementation.</summary>
    protected virtual CreationContext RebuildContextForParameter(CreationContext current, Type parameterType)
    {
        return parameterType.GetTypeInfo().ContainsGenericParameters
            ? current
            : new CreationContext(parameterType, current, false);
    }

    [PublicAPI]
    protected virtual object? ResolveFromKernel(CreationContext context, ComponentModel model,
        DependencyModel dependency)
    {
        if (dependency.ReferencedComponentName != null)
        {
            return ResolveFromKernelByName(context, model, dependency);
        }

        if (dependency.Parameter != null)
        {
            return ResolveFromParameter(context, model, dependency.Parameter, dependency.TargetItemType);
        }

        if (typeof(IKernel).IsAssignableFrom(dependency.TargetItemType))
        {
            return _kernel;
        }

        // we can shortcircuit it here, since we know we won't find any components for value type service as those are not legal.
        return dependency.TargetItemType.IsPrimitiveType()
            ? null
            : ResolveFromKernelByType(context, model, dependency);
    }

    private static bool CanResolveFromContext(CreationContext context, ISubDependencyResolver contextHandlerResolver,
        ComponentModel model,
        DependencyModel dependency)
    {
        return context != null && context.CanResolve(context, contextHandlerResolver, model, dependency);
    }

    private static bool CanResolveFromContextHandlerResolver(CreationContext context,
        ISubDependencyResolver contextHandlerResolver,
        ComponentModel model, DependencyModel dependency)
    {
        return contextHandlerResolver != null &&
               contextHandlerResolver.CanResolve(context, contextHandlerResolver, model, dependency);
    }

    private bool CanResolveFromHandler(CreationContext context, ISubDependencyResolver contextHandlerResolver,
        ComponentModel model,
        DependencyModel dependency)
    {
        var handler = _kernel.GetHandler(model.Name);
        var b = handler != null && handler != contextHandlerResolver &&
                handler.CanResolve(context, contextHandlerResolver, model, dependency);
        return b;
    }

    private bool CanResolveFromSubResolvers(CreationContext context, ISubDependencyResolver contextHandlerResolver,
        ComponentModel model,
        DependencyModel dependency)
    {
        return _subResolvers.Count > 0 &&
               _subResolvers.Any(s => s.CanResolve(context, contextHandlerResolver, model, dependency));
    }

    private bool HasAnyComponentInValidState(Type service, DependencyModel dependency, CreationContext context)
    {
        var firstHandler = context is { IsResolving: true }
            ? _kernel.LoadHandlerByType(dependency.DependencyKey, service, context.AdditionalArguments)
            : _kernel.GetHandler(service);

        if (firstHandler == null)
        {
            return false;
        }

        if (context == null || !firstHandler.IsBeingResolvedInContext(context))
        {
            if (IsHandlerInValidState(firstHandler))
            {
                return true;
            }
        }

        var handlers = _kernel.GetHandlers(service);
        var nonResolvingHandlers =
            handlers.Where(handler => !handler.IsBeingResolvedInContext(context)).ToList();
        RebuildOpenGenericHandlersWithClosedGenericSubHandlers(service, context, nonResolvingHandlers);
        return nonResolvingHandlers.Any(IsHandlerInValidState);
    }

    private void RebuildOpenGenericHandlersWithClosedGenericSubHandlers(Type service, CreationContext context,
        List<IHandler> nonResolvingHandlers)
    {
        if (context.RequestedType == null || !service.GetTypeInfo().IsGenericType)
        {
            return;
        }

        // Remove DefaultGenericHandlers
        var genericHandlers = nonResolvingHandlers.OfType<DefaultGenericHandler>().ToList();
        nonResolvingHandlers.RemoveAll(x => genericHandlers.Contains(x));

        // Convert open generic handlers to closed generic sub handlers 
        var openGenericContext = RebuildContextForParameter(context, service);
        var closedGenericSubHandlers = genericHandlers
            .Select(x => x.ConvertToClosedGenericHandler(service, openGenericContext)).ToList();

        // Update nonResolvingHandlers with closed generic sub handlers with potentially valid state
        nonResolvingHandlers.AddRange(closedGenericSubHandlers);
    }

    private bool HasComponentInValidState(string key, DependencyModel dependency, CreationContext context)
    {
        var handler = context is { IsResolving: true }
            ? _kernel.LoadHandlerByName(key, dependency.TargetItemType, context.AdditionalArguments)
            : _kernel.GetHandler(key);
        
        return IsHandlerInValidState(handler) && !handler.IsBeingResolvedInContext(context);
    }

    private bool TryResolveCore(CreationContext context, ISubDependencyResolver contextHandlerResolver,
        ComponentModel model, DependencyModel dependency, out object value)
    {
        // 1 - check for the dependency on CreationContext, if present
        if (CanResolveFromContext(context, contextHandlerResolver, model, dependency))
        {
            value = context.Resolve(context, contextHandlerResolver, model, dependency);
            return true;
        }

        // 2 - check with the model's handler, if not the same as the parent resolver
        var handler = _kernel.GetHandler(model.Name);
        if (handler != contextHandlerResolver && handler.CanResolve(context, contextHandlerResolver, model, dependency))
        {
            value = handler.Resolve(context, contextHandlerResolver, model, dependency);
            return true;
        }

        // 3 - check within parent resolver, if present
        if (CanResolveFromContextHandlerResolver(context, contextHandlerResolver, model, dependency))
        {
            value = contextHandlerResolver.Resolve(context, contextHandlerResolver, model, dependency);
            return true;
        }

        // 4 - check within subresolvers
        if (_subResolvers.Count > 0)
        {
            foreach (var subResolver in _subResolvers.Where(subResolver =>
                         subResolver.CanResolve(context, contextHandlerResolver, model, dependency)))
            {
                value = subResolver.Resolve(context, contextHandlerResolver, model, dependency);
                return true;
            }
        }

        // 5 - normal flow, checking against the kernel
        value = ResolveFromKernel(context, model, dependency);
        return value != null;
    }

    private object ResolveFromKernelByName(CreationContext context, ComponentModel model, DependencyModel dependency)
    {
        var handler = _kernel.LoadHandlerByName(dependency.ReferencedComponentName, dependency.TargetItemType,
            context.AdditionalArguments);

        // never (famous last words) this should really happen as we're the good guys and we call CanResolve before trying to resolve but let's be safe.
        if (handler == null)
        {
            throw new DependencyResolverException(
                string.Format(
                    "Missing dependency.{2}Component {0} has a dependency on component {1}, which was not registered.{2}Make sure the dependency is correctly registered in the container as a service.",
                    model.Name,
                    dependency.ReferencedComponentName,
                    Environment.NewLine));
        }

        var contextRebuilt = RebuildContextForParameter(context, dependency.TargetItemType);

        return handler.Resolve(contextRebuilt);
    }

    private object? ResolveFromKernelByType(CreationContext context, ComponentModel model, DependencyModel dependency)
    {
        if (!TryGetHandlerFromKernel(dependency, context, out var handler))
        {
            if (dependency.HasDefaultValue)
            {
                return dependency.DefaultValue;
            }

            throw new DependencyResolverException(
                string.Format(
                    "Missing dependency.{2}Component {0} has a dependency on {1}, which could not be resolved.{2}Make sure the dependency is correctly registered in the container as a service, or provided as inline argument.",
                    model.Name,
                    dependency.TargetItemType,
                    Environment.NewLine));
        }

        if (handler == null)
        {
            if (dependency.HasDefaultValue)
            {
                return dependency.DefaultValue;
            }

            throw new DependencyResolverException(
                string.Format(
                    "Cycle detected in configuration.{2}Component {0} has a dependency on {1}, but it doesn't provide an override.{2}You must provide an override if a component has a dependency on a service that it - itself - provides.",
                    model.Name,
                    dependency.TargetItemType,
                    Environment.NewLine));
        }

        context = RebuildContextForParameter(context, dependency.TargetItemType);

        return handler.Resolve(context);
    }

    private object ResolveFromParameter(CreationContext context, ComponentModel model, ParameterModel parameter, Type targetItemType)
    {
        _converter.Context.Push(model, context);
        
        try
        {
            if (parameter.Value != null || parameter.ConfigValue == null)
            {
                return _converter.PerformConversion(parameter.Value, targetItemType);
            }
            else
            {
                return _converter.PerformConversion(parameter.ConfigValue, targetItemType);
            }
        }
        catch (ConverterException e)
        {
            throw new DependencyResolverException(
                $"Could not convert parameter '{parameter.Name}' to type '{targetItemType.Name}'.",
                e);
        }
        finally
        {
            _converter.Context.Pop();
        }
    }

    private bool TryGetHandlerFromKernel(DependencyModel dependency, CreationContext context, out IHandler? handler)
    {
        // we are doing it in two stages because it is likely to be faster to a lookup
        // by key than a linear search
        try
        {
            handler = _kernel.LoadHandlerByType(dependency.DependencyKey, dependency.TargetItemType,
                context.AdditionalArguments);
        }
        catch (HandlerException)
        {
            handler = null;
        }

        if (handler == null)
        {
            return false;
        }

        if (!handler.IsBeingResolvedInContext(context))
        {
            return true;
        }

        // make a best effort to find another one that fit

        IHandler[] handlers;
        try
        {
            handlers = _kernel.GetHandlers(dependency.TargetItemType);
        }
        catch (HandlerException)
        {
            return false;
        }

        foreach (var maybeCorrectHandler in handlers)
        {
            if (maybeCorrectHandler.IsBeingResolvedInContext(context))
            {
                continue;
            }

            handler = maybeCorrectHandler;
            break;
        }

        return true;
    }

    private static bool IsHandlerInValidState([NotNullWhen(true)] IHandler? handler)
    {
        if (handler == null)
        {
            return false;
        }

        return handler.CurrentState == HandlerState.Valid;
    }
}