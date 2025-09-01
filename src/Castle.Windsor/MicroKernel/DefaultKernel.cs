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

using System.Diagnostics;
using System.Reflection;
using Castle.Core.Logging;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel.ComponentActivator;
using Castle.Windsor.MicroKernel.Context;
using Castle.Windsor.MicroKernel.Handlers;
using Castle.Windsor.MicroKernel.Lifestyle;
using Castle.Windsor.MicroKernel.Lifestyle.Scoped;
using Castle.Windsor.MicroKernel.ModelBuilder;
using Castle.Windsor.MicroKernel.ModelBuilder.Inspectors;
using Castle.Windsor.MicroKernel.Proxy;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Releasers;
using Castle.Windsor.MicroKernel.Resolvers;
using Castle.Windsor.MicroKernel.SubSystems.Configuration;
using Castle.Windsor.MicroKernel.SubSystems.Conversion;
using Castle.Windsor.MicroKernel.SubSystems.Naming;
using Castle.Windsor.MicroKernel.SubSystems.Resource;
using Castle.Windsor.Windsor.Diagnostics;

namespace Castle.Windsor.MicroKernel;

/// <summary>
///     Default implementation of <see cref="IKernel" />. This implementation is complete and also support a kernel
///     hierarchy (sub containers).
/// </summary>
[Serializable]
[DebuggerTypeProxy(typeof(KernelDebuggerProxy))]
public sealed partial class DefaultKernel :
    IKernelInternal
{
    [ThreadStatic] private static CreationContext? _currentCreationContext;

    [ThreadStatic] private static bool _isCheckingLazyLoaders;

    /// <summary>List of sub containers.</summary>
    private readonly List<IKernel> _childKernels = [];

    /// <summary>List of <see cref="IFacility" /> registered.</summary>
    private readonly List<IFacility> _facilities = [];

    private readonly Lock _lazyLoadingLock = new();

    /// <summary>Map of subsystems registered.</summary>
    private readonly Dictionary<string, ISubSystem> _subsystems = new(StringComparer.OrdinalIgnoreCase);

    // ReSharper disable once UnassignedField.Compiler
    private ThreadSafeFlag _disposed;

    /// <summary>The parent kernel, if exists.</summary>
    private IKernel? _parentKernel;

    /// <summary>Constructs a DefaultKernel with no component proxy support.</summary>
    public DefaultKernel() : this(new NotSupportedProxyFactory())
    {
    }

    /// <summary>
    ///     Constructs a DefaultKernel with the specified implementation of <see cref="IProxyFactory" /> and
    ///     <see cref="IDependencyResolver" />
    /// </summary>
    /// <param name="resolver"> </param>
    /// <param name="proxyFactory"> </param>
    public DefaultKernel(IDependencyResolver resolver, IProxyFactory proxyFactory)
    {
        RegisterSubSystems();
        ReleasePolicy = new LifecycledComponentsReleasePolicy(this);
        HandlerFactory = new DefaultHandlerFactory(this);
        ComponentModelBuilder = new DefaultComponentModelBuilder(this);
        ProxyFactory = proxyFactory;
        Resolver = resolver;
        Resolver.Initialize(this, RaiseDependencyResolving);

        {
            Logger = NullLogger.Instance;
        }
    }

    /// <summary>Constructs a DefaultKernel with the specified implementation of <see cref="IProxyFactory" /></summary>
    public DefaultKernel(IProxyFactory proxyFactory)
        : this(new DefaultDependencyResolver(), proxyFactory)
    {
    }

    private IConversionManager? ConversionSubSystem { get; set; }

    private INamingSubSystem? NamingSubSystem { get; set; }

    public IComponentModelBuilder ComponentModelBuilder { get; set; }

    public IConfigurationStore ConfigurationStore
    {
        get => GetSubSystem<IConfigurationStore>(SubSystemConstants.ConfigurationStoreKey);
        set => AddSubSystem(SubSystemConstants.ConfigurationStoreKey, value);
    }

    /// <summary>Graph of components and interactions.</summary>
    public GraphNode[] GraphNodes
    {
        get
        {
            var nodes = new GraphNode[NamingSubSystem?.ComponentCount ?? 0];
            var index = 0;

            var handlers = NamingSubSystem?.GetAllHandlers() ?? [];
            foreach (var handler in handlers)
            {
                nodes[index++] = handler.ComponentModel;
            }

            return nodes;
        }
    }

    public IHandlerFactory HandlerFactory { get; private set; }

    public IKernel? Parent
    {
        get => _parentKernel;
        set
        {
            // TODO: should the raise add/removed as child kernel methods be invoked from within the subscriber/unsubscribe methods?

            if (value == null)
            {
                if (_parentKernel != null)
                {
                    UnsubscribeFromParentKernel();
                    RaiseRemovedAsChildKernel();
                }

                _parentKernel = null;
            }
            else
            {
                if (_parentKernel != value && _parentKernel != null)
                {
                    throw new KernelException(
                        "You can not change the kernel parent once set, use the RemoveChildKernel and AddChildKernel methods together to achieve this.");
                }

                _parentKernel = value;
                SubscribeToParentKernel();
                RaiseAddedAsChildKernel();
            }
        }
    }

    public IProxyFactory ProxyFactory { get; set; }

    public IReleasePolicy ReleasePolicy { get; set; }

    public IDependencyResolver Resolver { get; private set; }


    /// <summary>Starts the process of component disposal.</summary>
    public void Dispose()
    {
        if (!_disposed.Signal())
        {
            return;
        }

        DisposeSubKernels();
        TerminateFacilities();
        DisposeHandlers();
        DisposeComponentsInstancesWithinTracker();
        UnsubscribeFromParentKernel();
    }

    public void AddChildKernel(IKernel childKernel)
    {
        ArgumentNullException.ThrowIfNull(childKernel);

        childKernel.Parent = this;
        _childKernels.Add(childKernel);
    }

    public IHandler AddCustomComponent(ComponentModel model)
    {
        var handler = (this as IKernelInternal).CreateHandler(model);
        NamingSubSystem?.Register(handler);
        (this as IKernelInternal).RaiseEventsOnHandlerCreated(handler);
        return handler;
    }

    IHandler IKernelInternal.CreateHandler(ComponentModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        RaiseComponentModelCreated(model);
        return HandlerFactory.Create(model);
    }

    public ILogger Logger { get; set; }

    public IKernel AddFacility(IFacility facility)
    {
        // ArgumentNullException.ThrowIfNull(facility);

        if (ConfigurationStore is null)
        {
            throw new InvalidOperationException(
                "Cannot add a facility to the container when no configuration store is available. " +
                "Please make sure that the container is configured with a configuration store.");
        }

        var facilityType = facility.GetType();
        if (facilityType.FullName is null)
        {
            throw new InvalidOperationException(
                "Cannot add a facility to the container when the facility type does not have a full name. " +
                "Please make sure that the facility type has a full name.");
        }

        if (_facilities.Any(f => f.GetType() == facilityType))
        {
            throw new ArgumentException(
                $"Facility of type '{facilityType.FullName}' has already been registered with the container. Only one facility of a given type can exist in the container.");
        }

        _facilities.Add(facility);
        facility.Init(this, ConfigurationStore.GetFacilityConfiguration(facilityType.FullName));

        return this;
    }

    public IKernel AddFacility<T>() where T : IFacility, new()
    {
        return AddFacility(new T());
    }

    public IKernel AddFacility<T>(Action<T>? onCreate)
        where T : IFacility, new()
    {
        var facility = new T();
        onCreate?.Invoke(facility);

        return AddFacility(facility);
    }

    public void AddHandlerSelector(IHandlerSelector selector)
    {
        NamingSubSystem?.AddHandlerSelector(selector);
    }

    public void AddHandlersFilter(IHandlersFilter filter)
    {
        NamingSubSystem?.AddHandlersFilter(filter);
    }

    public void AddSubSystem(string name, ISubSystem subsystem)
    {
        subsystem.Init(this);
        _subsystems[name] = subsystem;
        switch (name)
        {
            case SubSystemConstants.ConversionManagerKey:
                ConversionSubSystem = (IConversionManager)subsystem;
                break;
            case SubSystemConstants.NamingKey:
                NamingSubSystem = (INamingSubSystem)subsystem;
                break;
        }
    }

    /// <summary>Return handlers for components that implements the specified service. The check is made using IsAssignableFrom</summary>
    /// <param name="service"> </param>
    /// <returns> </returns>
    public IHandler[] GetAssignableHandlers(Type service)
    {
        if (NamingSubSystem == null)
        {
            throw new InvalidOperationException("The kernel does not have a naming subsystem.");
        }

        var result = NamingSubSystem.GetAssignableHandlers(service);

        // If a parent kernel exists, we merge both results
        if (Parent == null)
        {
            return result;
        }

        var parentResult = Parent.GetAssignableHandlers(service);

        if (parentResult.Length <= 0)
        {
            return result;
        }

        var newResult = new IHandler[result.Length + parentResult.Length];
        result.CopyTo(newResult, 0);
        parentResult.CopyTo(newResult, result.Length);
        result = newResult;

        return result;
    }

    /// <summary>Returns the facilities registered on the kernel.</summary>
    /// <returns> </returns>
    public IFacility[] GetFacilities()
    {
        return _facilities.ToArray();
    }

    public IHandler? GetHandler(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (NamingSubSystem == null)
        {
            throw new InvalidOperationException("The kernel does not have a naming subsystem.");
        }

        var handler = NamingSubSystem.GetHandler(name);

        if (handler == null && Parent != null)
        {
            handler = WrapParentHandler(Parent.GetHandler(name));
        }

        return handler;
    }

    public IHandler? GetHandler(Type service)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (NamingSubSystem == null)
        {
            throw new InvalidOperationException("The kernel does not have a naming subsystem.");
        }

        var handler = NamingSubSystem.GetHandler(service);
        if (handler == null && Parent != null)
        {
            handler = WrapParentHandler(Parent.GetHandler(service));
        }

        return handler;
    }

    /// <summary>Return handlers for components that implements the specified service.</summary>
    /// <param name="service"> </param>
    /// <returns> </returns>
    public IHandler[] GetHandlers(Type? service)
    {
        if (NamingSubSystem == null)
        {
            throw new InvalidOperationException("The kernel does not have a naming subsystem.");
        }

        var result = NamingSubSystem.GetHandlers(service);

        // If a parent kernel exists, we merge both results
        if (Parent == null)
        {
            return result;
        }

        var parentResult = Parent.GetHandlers(service);

        if (parentResult.Length <= 0)
        {
            return result;
        }

        var newResult = new IHandler[result.Length + parentResult.Length];
        result.CopyTo(newResult, 0);
        parentResult.CopyTo(newResult, result.Length);
        result = newResult;

        return result;
    }

    /// <summary>Returns all handlers for all components</summary>
    /// <returns>Handler which is a sub dependency resolver for a component</returns>
    public IHandler[] GetHandlers()
    {
        if (NamingSubSystem == null)
        {
            throw new InvalidOperationException("The kernel does not have a naming subsystem.");
        }

        var result = NamingSubSystem.GetAllHandlers();

        // If a parent kernel exists, we merge both results
        if (Parent == null)
        {
            return result;
        }

        var parentResult = Parent.GetHandlers();

        if (parentResult.Length <= 0)
        {
            return result;
        }

        var newResult = new IHandler[result.Length + parentResult.Length];
        result.CopyTo(newResult, 0);
        parentResult.CopyTo(newResult, result.Length);
        result = newResult;

        return result;
    }

    public TSubSystem GetSubSystem<TSubSystem>(string name) where TSubSystem : class
    {
        _subsystems.TryGetValue(name, out var subsystem);
        return subsystem as TSubSystem ??
               throw new InvalidOperationException($"The kernel does not have a subsystem named '{name}' of type '{typeof(TSubSystem).FullName}'.");
    }

    public bool HasComponent(string? name)
    {
        if (name == null)
        {
            return false;
        }

        if (NamingSubSystem == null)
        {
            throw new InvalidOperationException("The kernel does not have a naming subsystem.");
        }

        if (NamingSubSystem.Contains(name))
        {
            return true;
        }

        return Parent != null && Parent.HasComponent(name);
    }

    public bool HasComponent(Type? serviceType)
    {
        if (serviceType == null)
        {
            return false;
        }

        if (NamingSubSystem == null)
        {
            throw new InvalidOperationException("The kernel does not have a naming subsystem.");
        }

        if (NamingSubSystem.Contains(serviceType))
        {
            return true;
        }

        return Parent != null && Parent.HasComponent(serviceType);
    }

    /// <summary>
    ///     Registers the components with the <see cref="IKernel" />. The instances of <see cref="IRegistration" /> are
    ///     produced by fluent registration API. Most common entry points are
    ///     <see cref="Component.For{TService}" /> method to register a single type or (recommended in most cases)
    ///     <see cref="Classes.FromAssembly(Assembly)" />. Let the Intellisense drive you through
    ///     the fluent API past those entry points.
    /// </summary>
    /// <example>
    ///     <code>kernel.Register(Component.For&lt;IService&gt;().ImplementedBy&lt;DefaultService&gt;().LifestyleTransient());</code>
    /// </example>
    /// <example>
    ///     <code>kernel.Register(Classes.FromThisAssembly().BasedOn&lt;IService&gt;().WithServiceDefaultInterfaces().Configure(c => c.LifestyleTransient()));</code>
    /// </example>
    /// <param name="registrations">
    ///     The component registrations created by <see cref="Component.For{TService}" /> ,
    ///     <see cref="Classes.FromAssembly(Assembly)" /> or different entry method to the fluent
    ///     API.
    /// </param>
    /// <returns> The kernel. </returns>
    public IKernel Register(params IRegistration[] registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);

        var token = OptimizeDependencyResolution();
        foreach (var registration in registrations)
        {
            registration.Register(this);
        }

        token?.Dispose();
        return this;
    }

    /// <summary>
    ///     Releases a component instance. This allows the kernel to execute the proper decommission lifecycles on the
    ///     component instance.
    /// </summary>
    /// <param name="instance"> </param>
    public void ReleaseComponent(object instance)
    {
        if (ReleasePolicy.HasTrack(instance))
        {
            ReleasePolicy.Release(instance);
        }
        else
        {
            Parent?.ReleaseComponent(instance);
        }
    }

    public void RemoveChildKernel(IKernel childKernel)
    {
        ArgumentNullException.ThrowIfNull(childKernel);

        childKernel.Parent = null;
        _childKernels.Remove(childKernel);
    }

    /// <summary>
    ///     Creates an implementation of <see cref="ILifestyleManager" /> based on <see cref="LifestyleType" /> and invokes
    ///     <see cref="ILifestyleManager.Init" /> to initialize the newly created
    ///     manager.
    /// </summary>
    /// <param name="model"> </param>
    /// <param name="activator"> </param>
    /// <returns> </returns>
    public ILifestyleManager CreateLifestyleManager(ComponentModel model, IComponentActivator activator)
    {
        ILifestyleManager manager;
        var type = model.LifestyleType;

        switch (type)
        {
            case LifestyleType.Scoped:
                manager = new ScopedLifestyleManager(CreateScopeAccessor(model));
                break;
            case LifestyleType.Bound:
                manager = new ScopedLifestyleManager(CreateScopeAccessorForBoundLifestyle(model));
                break;
            case LifestyleType.Thread:
                manager = new ScopedLifestyleManager(new ThreadScopeAccessor());
                break;
            case LifestyleType.Transient:
                manager = new TransientLifestyleManager();
                break;
            case LifestyleType.Custom:
                manager = (model.CustomLifestyle ?? throw new InvalidOperationException())
                    .CreateInstance<ILifestyleManager>();
                break;
            case LifestyleType.Pooled:
                var initial = ExtendedPropertiesConstants.PoolDefaultInitialPoolSize;
                var maxSize = ExtendedPropertiesConstants.PoolDefaultMaxPoolSize;

                if (model.ExtendedProperties.Contains(ExtendedPropertiesConstants.PoolInitialPoolSize))
                {
                    initial = (int)(model.ExtendedProperties[ExtendedPropertiesConstants.PoolInitialPoolSize] ??
                                    throw new InvalidOperationException());
                }

                if (model.ExtendedProperties.Contains(ExtendedPropertiesConstants.PoolMaxPoolSize))
                {
                    maxSize = (int)(model.ExtendedProperties[ExtendedPropertiesConstants.PoolMaxPoolSize] ??
                                    throw new InvalidOperationException());
                }

                manager = new PoolableLifestyleManager(initial, maxSize);
                break;
            case LifestyleType.Undefined:
            case LifestyleType.Singleton:
            default:
                //this includes LifestyleType.Undefined, LifestyleType.Singleton and invalid values
                manager = new SingletonLifestyleManager();
                break;
        }

        manager.Init(activator, this, model);

        return manager;
    }

    public IComponentActivator CreateComponentActivator(ComponentModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        IComponentActivator activator;

        if (model.CustomComponentActivator == null)
        {
            activator = new DefaultComponentActivator(model, this,
                RaiseComponentCreated,
                RaiseComponentDestroyed);
        }
        else
        {
            try
            {
                activator = model.CustomComponentActivator.CreateInstance<IComponentActivator>(model, this,
                    new ComponentInstanceDelegate(RaiseComponentCreated),
                    new ComponentInstanceDelegate(RaiseComponentDestroyed));
            }
            catch (Exception e)
            {
                throw new KernelException("Could not instantiate custom activator", e);
            }
        }

        return activator;
    }

    void IKernelInternal.RaiseEventsOnHandlerCreated(IHandler handler)
    {
        RaiseHandlerRegistered(handler);
        RaiseHandlersChanged();
        RaiseComponentRegistered(handler.ComponentModel.Name, handler);
    }

    IHandler? IKernelInternal.LoadHandlerByName(string name, Type? service, Arguments? arguments)
    {
        var handler = GetHandler(name);
        if (handler != null)
        {
            return handler;
        }

        lock (_lazyLoadingLock)
        {
            handler = GetHandler(name);
            if (handler != null)
            {
                return handler;
            }

            if (_isCheckingLazyLoaders)
            {
                return null;
            }

            _isCheckingLazyLoaders = true;
            try
            {
                foreach (var loader in ResolveAll<ILazyComponentLoader>())
                {
                    var registration = loader.Load(name, service, arguments);
                    if (registration == null)
                    {
                        continue;
                    }

                    registration.Register(this);
                    return GetHandler(name);
                }

                return null;
            }
            finally
            {
                _isCheckingLazyLoaders = false;
            }
        }
    }

    IHandler? IKernelInternal.LoadHandlerByType(string? name, Type service, Arguments? arguments)
    {
        ArgumentNullException.ThrowIfNull(service);

        var handler = GetHandler(service);
        if (handler != null)
        {
            return handler;
        }

        lock (_lazyLoadingLock)
        {
            handler = GetHandler(service);
            if (handler != null)
            {
                return handler;
            }

            if (_isCheckingLazyLoaders)
            {
                return null;
            }

            _isCheckingLazyLoaders = true;
            try
            {
                foreach (var loader in ResolveAll<ILazyComponentLoader>())
                {
                    var registration = loader.Load(name, service, arguments);
                    if (registration == null)
                    {
                        continue;
                    }

                    registration.Register(this);
                    return GetHandler(service);
                }

                return null;
            }
            finally
            {
                _isCheckingLazyLoaders = false;
            }
        }
    }

    private static IScopeAccessor CreateScopeAccessor(ComponentModel model)
    {
        var scopeAccessorType = model.GetScopeAccessorType();
        return scopeAccessorType == null
            ? new LifetimeScopeAccessor()
            : scopeAccessorType.CreateInstance<IScopeAccessor>();
    }

    private static CreationContextScopeAccessor CreateScopeAccessorForBoundLifestyle(ComponentModel model)
    {
        var selector = (Func<IHandler[], IHandler>?)model.ExtendedProperties[Constants.ScopeRootSelector];
        if (selector == null)
        {
            throw new ComponentRegistrationException(
                $"Component {model.Name} has lifestyle {LifestyleType.Bound} but it does not specify mandatory 'scopeRootSelector'.");
        }

        return new CreationContextScopeAccessor(model, selector);
    }

    private CreationContext CreateCreationContext(IHandler handler, Type requestedType, Arguments? additionalArguments,
        CreationContext? parent,
        IReleasePolicy policy)
    {
        return new CreationContext(handler, policy, requestedType, additionalArguments, ConversionSubSystem, parent);
    }

    /// <remarks>It is the responsibility of the kernel to ensure that handler is only ever disposed once.</remarks>
    private static void DisposeHandler(IHandler? handler)
    {
        var disposable = handler as IDisposable;

        disposable?.Dispose();
    }

    private void RegisterSubSystems()
    {
        AddSubSystem(SubSystemConstants.ConfigurationStoreKey,
            new DefaultConfigurationStore());

        AddSubSystem(SubSystemConstants.ConversionManagerKey,
            new DefaultConversionManager());

        AddSubSystem(SubSystemConstants.NamingKey,
            new DefaultNamingSubSystem());

        AddSubSystem(SubSystemConstants.ResourceKey,
            new DefaultResourceSubSystem());

        AddSubSystem(SubSystemConstants.DiagnosticsKey,
            new DefaultDiagnosticsSubSystem());
    }

    private object ResolveComponent(IHandler handler, Type service, Arguments? additionalArguments,
        IReleasePolicy policy, bool ignoreParentContext = false)
    {
        Debug.Assert(handler != null);
        var parent = _currentCreationContext;
        var context = CreateCreationContext(handler, service, additionalArguments, ignoreParentContext ? null : parent,
            policy);

        _currentCreationContext = context;
        try
        {
            return handler.Resolve(context);
        }
        finally
        {
            _currentCreationContext = parent;
        }
    }

    private ParentHandlerWrapper? WrapParentHandler(IHandler? parentHandler)
    {
        if (parentHandler == null)
        {
            return null;
        }

        if (Parent == null)
        {
            throw new InvalidOperationException("The parent kernel does not exist.");
        }

        var handler = new ParentHandlerWrapper(parentHandler, Parent.Resolver, Parent.ReleasePolicy);
        handler.Init(this);
        return handler;
    }

    private void DisposeComponentsInstancesWithinTracker()
    {
        ReleasePolicy.Dispose();
    }

    private void DisposeHandlers()
    {
        var vertices = TopologicalSortAlgo.Sort(GraphNodes);

        if (vertices.Length == 0)
        {
            return;
        }

        if (NamingSubSystem == null)
        {
            throw new InvalidOperationException("The kernel does not have a naming subsystem.");
        }

        foreach (var t in vertices)
        {
            var model = (ComponentModel)t;
            var handler = NamingSubSystem.GetHandler(model.Name);
            DisposeHandler(handler);
        }
    }

    private void DisposeSubKernels()
    {
        foreach (var childKernel in _childKernels)
        {
            childKernel.Dispose();
        }
    }

    private void HandlerRegisteredOnParentKernel(IHandler handler, ref bool stateChanged)
    {
        RaiseHandlerRegistered(handler);
    }

    private void HandlersChangedOnParentKernel(ref bool changed)
    {
        RaiseHandlersChanged();
    }

    private void SubscribeToParentKernel()
    {
        if (_parentKernel == null)
        {
            return;
        }

        _parentKernel.HandlerRegistered += HandlerRegisteredOnParentKernel;
        _parentKernel.HandlersChanged += HandlersChangedOnParentKernel;
        _parentKernel.ComponentRegistered += RaiseComponentRegistered;
    }

    private void TerminateFacilities()
    {
        foreach (var facility in _facilities)
        {
            facility.Terminate();
        }
    }

    private void UnsubscribeFromParentKernel()
    {
        if (_parentKernel == null)
        {
            return;
        }

        _parentKernel.HandlerRegistered -= HandlerRegisteredOnParentKernel;
        _parentKernel.HandlersChanged -= HandlersChangedOnParentKernel;
        _parentKernel.ComponentRegistered -= RaiseComponentRegistered;
    }
}