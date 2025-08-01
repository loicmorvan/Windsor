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

using System.Diagnostics;
using System.Reflection;
using System.Text;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel.ComponentActivator;
using Castle.Windsor.MicroKernel.Releasers;
using Castle.Windsor.MicroKernel.SubSystems.Conversion;

namespace Castle.Windsor.MicroKernel.Context;

/// <summary>
///     Used during a component request, passed along to the whole process. This allow some data to be passed along the
///     process, which is used to detected cycled dependency graphs and now it's also being
///     used to provide arguments to components.
/// </summary>
[Serializable]
public class CreationContext :
    ISubDependencyResolver
{
    private readonly ITypeConverter _converter;

    /// <summary>
    ///     The list of handlers that are used to resolve the component. We track that in order to try to avoid attempts
    ///     to resolve a service with itself.
    /// </summary>
    private readonly Stack<IHandler> _handlerStack;

    private readonly Stack<ResolutionContext> _resolutionStack;
    private Arguments _additionalArguments;
    private Arguments _extendedProperties;
    private Type[] _genericArguments;
    private bool _isResolving = true;

    /// <summary>Initializes a new instance of the <see cref="CreationContext" /> class.</summary>
    /// <param name="requestedType"> The type to extract generic arguments. </param>
    /// <param name="parentContext"> The parent context. </param>
    /// <param name="propagateInlineDependencies">
    ///     When set to <c>true</c> will clone <paramref name="parentContext" />
    ///     <see cref="AdditionalArguments" /> .
    /// </param>
    public CreationContext(Type requestedType, CreationContext parentContext, bool propagateInlineDependencies)
        : this(parentContext.Handler, parentContext.ReleasePolicy, requestedType, null, null, parentContext)
    {
        ArgumentNullException.ThrowIfNull(parentContext);

        if (parentContext._extendedProperties != null)
        {
            _extendedProperties = new Arguments(parentContext._extendedProperties);
        }

        if (propagateInlineDependencies && parentContext.HasAdditionalArguments)
        {
            _additionalArguments = new Arguments(parentContext._additionalArguments);
        }
    }

    /// <summary>Initializes a new instance of the <see cref="CreationContext" /> class.</summary>
    /// <param name="handler"> The handler. </param>
    /// <param name="releasePolicy"> The release policy. </param>
    /// <param name="requestedType"> The type to extract generic arguments. </param>
    /// <param name="additionalArguments"> The additional arguments. </param>
    /// <param name="converter"> The conversion manager. </param>
    /// <param name="parent"> Parent context </param>
    public CreationContext(IHandler handler, IReleasePolicy releasePolicy, Type requestedType,
        Arguments additionalArguments, ITypeConverter converter,
        CreationContext parent)
    {
        RequestedType = requestedType;
        Handler = handler;
        ReleasePolicy = releasePolicy;
        _additionalArguments = additionalArguments;
        _converter = converter;

        if (parent != null)
        {
            _resolutionStack = parent._resolutionStack;
            _handlerStack = parent._handlerStack;
            return;
        }

        _handlerStack = new Stack<IHandler>(4);
        _resolutionStack = new Stack<ResolutionContext>(4);
    }

    /// <summary>Initializes a new instance of the <see cref="CreationContext" /> class.</summary>
    private CreationContext()
    {
#pragma warning disable 612,618
        ReleasePolicy = new NoTrackingReleasePolicy();
#pragma warning restore 612,618
        _handlerStack = new Stack<IHandler>(4);
        _resolutionStack = new Stack<ResolutionContext>(4);
    }

    public Arguments AdditionalArguments => _additionalArguments ??= new Arguments();

    public Type[] GenericArguments => _genericArguments ??= ExtractGenericArguments(RequestedType);

    public IHandler Handler { get; }

    public bool HasAdditionalArguments => _additionalArguments != null && _additionalArguments.Count != 0;

    public virtual bool IsResolving => _isResolving;

    public IReleasePolicy ReleasePolicy { get; set; }

    public Type RequestedType { get; }

    public virtual bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
        ComponentModel model,
        DependencyModel dependency)
    {
        return HasAdditionalArguments && (CanResolveByKey(dependency) || CanResolveByType(dependency));
    }

    public virtual object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
        ComponentModel model,
        DependencyModel dependency)
    {
        Debug.Assert(CanResolve(context, contextHandlerResolver, model, dependency));
        object result = null;
        if (dependency.DependencyKey != null)
        {
            result = Resolve(dependency, _additionalArguments[dependency.DependencyKey]);
        }

        return result ?? Resolve(dependency, _additionalArguments[dependency.TargetType]);
    }

    public void AttachExistingBurden(Burden burden)
    {
        ResolutionContext resolutionContext;
        try
        {
            resolutionContext = _resolutionStack.Peek();
        }
        catch (InvalidOperationException)
        {
            throw new ComponentActivatorException(
                "Not in a resolution context. 'AttachExistingBurden' method can only be called withing a resoltion scope. (after 'EnterResolutionContext' was called within a handler)",
                null);
        }

        resolutionContext.AttachBurden(burden);
    }

    public void BuildCycleMessageFor(IHandler duplicateHandler, StringBuilder message)
    {
        message.Append($"Component '{duplicateHandler.ComponentModel.Name}'");

        foreach (var handlerOnTheStack in _handlerStack)
        {
            message.AppendFormat(" resolved as dependency of");
            message.AppendLine();
            message.Append($"\tcomponent '{handlerOnTheStack.ComponentModel.Name}'");
        }

        message.AppendLine(" which is the root component being resolved.");
    }

    public Burden CreateBurden(IComponentActivator componentActivator, bool trackedExternally)
    {
        ResolutionContext resolutionContext;
        try
        {
            resolutionContext = _resolutionStack.Peek();
        }
        catch (InvalidOperationException)
        {
            throw new ComponentActivatorException(
                "Not in a resolution context. 'CreateBurden' method can only be called withing a resoltion scope. (after 'EnterResolutionContext' was called within a handler)",
                null);
        }

        if (componentActivator is IDependencyAwareActivator activator)
        {
            trackedExternally |= activator.IsManagedExternally(resolutionContext.Handler.ComponentModel);
        }

        return resolutionContext.CreateBurden(trackedExternally);
    }

    public ResolutionContext EnterResolutionContext(IHandler handlerBeingResolved, bool requiresDecommission)
    {
        return EnterResolutionContext(handlerBeingResolved, true, requiresDecommission);
    }

    public ResolutionContext EnterResolutionContext(IHandler handlerBeingResolved, bool trackContext,
        bool requiresDecommission)
    {
        var resolutionContext = new ResolutionContext(this, handlerBeingResolved, requiresDecommission, trackContext);
        _handlerStack.Push(handlerBeingResolved);
        if (trackContext)
        {
            _resolutionStack.Push(resolutionContext);
        }

        return resolutionContext;
    }

    public object GetContextualProperty(object key)
    {
        return _extendedProperties?[key];
    }

    /// <summary>Method used by handlers to test whether they are being resolved in the context.</summary>
    /// <param name="handler"> </param>
    /// <returns> </returns>
    /// <remarks>
    ///     This method is provided as part of double dispatch mechanism for use by handlers. Outside of handlers, call
    ///     <see cref="IHandler.IsBeingResolvedInContext" /> instead.
    /// </remarks>
    public bool IsInResolutionContext(IHandler handler)
    {
        return _handlerStack.Contains(handler);
    }

    public ResolutionContext SelectScopeRoot(Func<IHandler[], IHandler> scopeRootSelector)
    {
        var scopes = _resolutionStack.Select(c => c.Handler).Reverse().ToArray();
        var selected = scopeRootSelector(scopes);
        if (selected == null)
        {
            return null;
        }

        var resolutionContext = _resolutionStack.SingleOrDefault(s => s.Handler == selected);
        return resolutionContext;
    }

    public void SetContextualProperty(object key, object value)
    {
        ArgumentNullException.ThrowIfNull(key);
        _extendedProperties ??= new Arguments();
        _extendedProperties[key] = value;
    }

    private bool CanConvertParameter(Type type)
    {
        return _converter != null && _converter.CanHandleType(type);
    }

    private bool CanResolve(DependencyModel dependency, object inlineArgument)
    {
        var type = dependency.TargetItemType;
        if (type == null)
        {
            return false;
        }

        if (inlineArgument == null)
        {
            return type.IsNullable();
        }

        return type.IsInstanceOfType(inlineArgument) || CanConvertParameter(type);
    }

    private bool CanResolveByKey(DependencyModel dependency)
    {
        if (dependency.DependencyKey == null)
        {
            return false;
        }

        Debug.Assert(_additionalArguments != null);
        return CanResolve(dependency, _additionalArguments[dependency.DependencyKey]);
    }

    private bool CanResolveByType(DependencyModel dependency)
    {
        var type = dependency.TargetItemType;
        if (type == null)
        {
            return false;
        }

        Debug.Assert(_additionalArguments != null);
        return CanResolve(dependency, _additionalArguments[type]);
    }

    private void ExitResolutionContext(Burden burden, bool trackContext)
    {
        _handlerStack.Pop();

        if (trackContext)
        {
            _resolutionStack.Pop();
        }

        if (burden?.Instance == null)
        {
            return;
        }

        if (burden.RequiresPolicyRelease == false)
        {
            return;
        }

        if (_resolutionStack.Count == 0)
        {
            return;
        }

        var parent = _resolutionStack.Peek().Burden;

        parent?.AddChild(burden);
    }

    private object Resolve(DependencyModel dependency, object inlineArgument)
    {
        var targetType = dependency.TargetItemType;
        if (inlineArgument == null)
        {
            return null;
        }

        if (targetType.IsInstanceOfType(inlineArgument))
        {
            return inlineArgument;
        }

        return CanConvertParameter(targetType)
            ? _converter.PerformConversion(inlineArgument.ToString(), targetType)
            : null;
    }

    /// <summary>Creates a new, empty <see cref="CreationContext" /> instance.</summary>
    /// <remarks>
    ///     A new CreationContext should be created every time, as the contexts keeps some state related to dependency
    ///     resolution.
    /// </remarks>
    public static CreationContext CreateEmpty()
    {
        return new CreationContext();
    }

    public static CreationContext ForDependencyInspection(IHandler handler)
    {
        var context = CreateEmpty();
        context._isResolving = false;
        context.EnterResolutionContext(handler, false);
        return context;
    }

    private static Type[] ExtractGenericArguments(Type typeToExtractGenericArguments)
    {
        return typeToExtractGenericArguments.GetTypeInfo().IsGenericType
            ? typeToExtractGenericArguments.GetTypeInfo().GetGenericArguments()
            : Type.EmptyTypes;
    }

    public class ResolutionContext(
        CreationContext context,
        IHandler handler,
        bool requiresDecommission,
        bool trackContext)
        : IDisposable
    {
        private Arguments _extendedProperties;

        public Burden Burden { get; private set; }

        private CreationContext Context { get; } = context;

        public IHandler Handler { get; } = handler;

        public void Dispose()
        {
            Context.ExitResolutionContext(Burden, trackContext);
        }

        public void AttachBurden(Burden burden)
        {
            Burden = burden;
        }

        public Burden CreateBurden(bool trackedExternally)
        {
            // NOTE: not sure we should allow crreating burden again, when it was already created...
            // this is currently employed by pooled lifestyle
            Burden = new Burden(Handler, requiresDecommission, trackedExternally);
            return Burden;
        }

        public object GetContextualProperty(object key)
        {
            var value = _extendedProperties?[key];
            return value;
        }

        public void SetContextualProperty(object key, object value)
        {
            ArgumentNullException.ThrowIfNull(key);
            _extendedProperties ??= new Arguments();
            _extendedProperties[key] = value;
        }
    }
}