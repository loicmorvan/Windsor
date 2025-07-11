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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Castle.Core;
using Castle.Core.Internal;
using Castle.Core.Logging;
using Castle.MicroKernel.ComponentActivator;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Handlers;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Lifestyle.Scoped;
using Castle.MicroKernel.ModelBuilder;
using Castle.MicroKernel.ModelBuilder.Inspectors;
using Castle.MicroKernel.Proxy;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Releasers;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.MicroKernel.SubSystems.Conversion;
using Castle.MicroKernel.SubSystems.Naming;
using Castle.MicroKernel.SubSystems.Resource;
using Castle.Windsor.Diagnostics;

namespace Castle.MicroKernel;

#if FEATURE_SECURITY_PERMISSIONS
	using System.Security.Permissions;
#endif

/// <summary>
///     Default implementation of <see cref="IKernel" />. This implementation is complete and also support a kernel
///     hierarchy (sub containers).
/// </summary>
[Serializable]
[DebuggerTypeProxy(typeof(KernelDebuggerProxy))]
public partial class DefaultKernel :
#if FEATURE_REMOTING
		MarshalByRefObject,
#endif
	IKernel, IKernelEvents, IKernelInternal
{
	[ThreadStatic] private static CreationContext _currentCreationContext;

	[ThreadStatic] private static bool _isCheckingLazyLoaders;

	// ReSharper disable once UnassignedField.Compiler
	private ThreadSafeFlag _disposed;

	/// <summary>
	///     List of sub containers.
	/// </summary>
	private readonly List<IKernel> _childKernels = [];

	/// <summary>
	///     List of <see cref="IFacility" /> registered.
	/// </summary>
	private readonly List<IFacility> _facilities = [];

	/// <summary>
	///     Map of subsystems registered.
	/// </summary>
	private readonly Dictionary<string, ISubSystem> _subsystems = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	///     The parent kernel, if exists.
	/// </summary>
	private IKernel _parentKernel;

	private readonly object _lazyLoadingLock = new();

	/// <summary>
	///     Constructs a DefaultKernel with no component proxy support.
	/// </summary>
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

#if FEATURE_SECURITY_PERMISSIONS
			if (new SecurityPermission(SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy).IsGranted())
			{
				Logger = new TraceLogger("Castle.Windsor", LoggerLevel.Warn);
			}
			else
#endif
		{
			Logger = NullLogger.Instance;
		}
	}

	/// <summary>
	///     Constructs a DefaultKernel with the specified implementation of <see cref="IProxyFactory" />
	/// </summary>
	public DefaultKernel(IProxyFactory proxyFactory)
		: this(new DefaultDependencyResolver(), proxyFactory)
	{
	}

#if FEATURE_SERIALIZATION
		[SecurityCritical]
		public DefaultKernel(SerializationInfo info, StreamingContext context)
		{
			var members = FormatterServices.GetSerializableMembers(GetType(), context);
			var kernelmembers = (object[])info.GetValue("members", typeof(object[]));

			FormatterServices.PopulateObjectMembers(this, members, kernelmembers);

			HandlerRegistered += (HandlerDelegate)info.GetValue("HandlerRegisteredEvent", typeof(Delegate));
		}
#endif

	public IComponentModelBuilder ComponentModelBuilder { get; set; }

	public virtual IConfigurationStore ConfigurationStore
	{
		get => GetSubSystem(SubSystemConstants.ConfigurationStoreKey) as IConfigurationStore;
		set => AddSubSystem(SubSystemConstants.ConfigurationStoreKey, value);
	}

	/// <summary>
	///     Graph of components and interactions.
	/// </summary>
	public GraphNode[] GraphNodes
	{
		get
		{
			var nodes = new GraphNode[NamingSubSystem.ComponentCount];
			var index = 0;

			var handlers = NamingSubSystem.GetAllHandlers();
			foreach (var handler in handlers) nodes[index++] = handler.ComponentModel;

			return nodes;
		}
	}

	public IHandlerFactory HandlerFactory { get; private set; }

	public virtual IKernel Parent
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
					throw new KernelException(
						"You can not change the kernel parent once set, use the RemoveChildKernel and AddChildKernel methods together to achieve this.");
				_parentKernel = value;
				SubscribeToParentKernel();
				RaiseAddedAsChildKernel();
			}
		}
	}

	public IProxyFactory ProxyFactory { get; set; }

	public IReleasePolicy ReleasePolicy { get; set; }

	public IDependencyResolver Resolver { get; private set; }

	protected IConversionManager ConversionSubSystem { get; private set; }

	protected INamingSubSystem NamingSubSystem { get; private set; }

#if FEATURE_SERIALIZATION
		[SecurityCritical]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			var members = FormatterServices.GetSerializableMembers(GetType(), context);

			var kernelmembers = FormatterServices.GetObjectData(this, members);

			info.AddValue("members", kernelmembers, typeof(object[]));

			info.AddValue("HandlerRegisteredEvent", HandlerRegistered);
		}
#endif

	/// <summary>
	///     Starts the process of component disposal.
	/// </summary>
	public virtual void Dispose()
	{
		if (!_disposed.Signal()) return;

		DisposeSubKernels();
		TerminateFacilities();
		DisposeHandlers();
		DisposeComponentsInstancesWithinTracker();
		UnsubscribeFromParentKernel();
	}

	public virtual void AddChildKernel(IKernel childKernel)
	{
		ArgumentNullException.ThrowIfNull(childKernel);

		childKernel.Parent = this;
		_childKernels.Add(childKernel);
	}

	public virtual IHandler AddCustomComponent(ComponentModel model)
	{
		var handler = (this as IKernelInternal).CreateHandler(model);
		NamingSubSystem.Register(handler);
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

	public virtual IKernel AddFacility(IFacility facility)
	{
		ArgumentNullException.ThrowIfNull(facility);
		var facilityType = facility.GetType();
		if (_facilities.Any(f => f.GetType() == facilityType))
			throw new ArgumentException(
				string.Format(
					"Facility of type '{0}' has already been registered with the container. Only one facility of a given type can exist in the container.",
					facilityType.FullName));
		_facilities.Add(facility);
		facility.Init(this, ConfigurationStore.GetFacilityConfiguration(facility.GetType().FullName));

		return this;
	}

	public IKernel AddFacility<T>() where T : IFacility, new()
	{
		return AddFacility(new T());
	}

	public IKernel AddFacility<T>(Action<T> onCreate)
		where T : IFacility, new()
	{
		var facility = new T();
		if (onCreate != null) onCreate(facility);
		return AddFacility(facility);
	}

	public void AddHandlerSelector(IHandlerSelector selector)
	{
		NamingSubSystem.AddHandlerSelector(selector);
	}

	public void AddHandlersFilter(IHandlersFilter filter)
	{
		NamingSubSystem.AddHandlersFilter(filter);
	}

	public virtual void AddSubSystem(string name, ISubSystem subsystem)
	{
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(subsystem);

		subsystem.Init(this);
		_subsystems[name] = subsystem;
		if (name == SubSystemConstants.ConversionManagerKey)
			ConversionSubSystem = (IConversionManager)subsystem;
		else if (name == SubSystemConstants.NamingKey) NamingSubSystem = (INamingSubSystem)subsystem;
	}

	/// <summary>
	///     Return handlers for components that implements the specified service. The check is made using IsAssignableFrom
	/// </summary>
	/// <param name="service"> </param>
	/// <returns> </returns>
	public virtual IHandler[] GetAssignableHandlers(Type service)
	{
		var result = NamingSubSystem.GetAssignableHandlers(service);

		// If a parent kernel exists, we merge both results
		if (Parent != null)
		{
			var parentResult = Parent.GetAssignableHandlers(service);

			if (parentResult.Length > 0)
			{
				var newResult = new IHandler[result.Length + parentResult.Length];
				result.CopyTo(newResult, 0);
				parentResult.CopyTo(newResult, result.Length);
				result = newResult;
			}
		}

		return result;
	}

	/// <summary>
	///     Returns the facilities registered on the kernel.
	/// </summary>
	/// <returns> </returns>
	public virtual IFacility[] GetFacilities()
	{
		return _facilities.ToArray();
	}

	public virtual IHandler GetHandler(string name)
	{
		ArgumentNullException.ThrowIfNull(name);

		var handler = NamingSubSystem.GetHandler(name);

		if (handler == null && Parent != null) handler = WrapParentHandler(Parent.GetHandler(name));

		return handler;
	}

	public virtual IHandler GetHandler(Type service)
	{
		ArgumentNullException.ThrowIfNull(service);

		var handler = NamingSubSystem.GetHandler(service);
		if (handler == null && Parent != null) handler = WrapParentHandler(Parent.GetHandler(service));

		return handler;
	}

	/// <summary>
	///     Return handlers for components that implements the specified service.
	/// </summary>
	/// <param name="service"> </param>
	/// <returns> </returns>
	public virtual IHandler[] GetHandlers(Type service)
	{
		var result = NamingSubSystem.GetHandlers(service);

		// If a parent kernel exists, we merge both results
		if (Parent != null)
		{
			var parentResult = Parent.GetHandlers(service);

			if (parentResult.Length > 0)
			{
				var newResult = new IHandler[result.Length + parentResult.Length];
				result.CopyTo(newResult, 0);
				parentResult.CopyTo(newResult, result.Length);
				result = newResult;
			}
		}

		return result;
	}

	/// <summary>
	///     Returns all handlers for all components
	/// </summary>
	/// <returns>Handler which is a sub dependency resolver for a component</returns>
	public virtual IHandler[] GetHandlers()
	{
		var result = NamingSubSystem.GetAllHandlers();

		// If a parent kernel exists, we merge both results
		if (Parent != null)
		{
			var parentResult = Parent.GetHandlers();

			if (parentResult.Length > 0)
			{
				var newResult = new IHandler[result.Length + parentResult.Length];
				result.CopyTo(newResult, 0);
				parentResult.CopyTo(newResult, result.Length);
				result = newResult;
			}
		}

		return result;
	}

	public virtual ISubSystem GetSubSystem(string name)
	{
		ISubSystem subsystem;
		_subsystems.TryGetValue(name, out subsystem);
		return subsystem;
	}

	public virtual bool HasComponent(string name)
	{
		if (name == null) return false;

		if (NamingSubSystem.Contains(name)) return true;

		if (Parent != null) return Parent.HasComponent(name);

		return false;
	}

	public virtual bool HasComponent(Type serviceType)
	{
		if (serviceType == null) return false;

		if (NamingSubSystem.Contains(serviceType)) return true;

		if (Parent != null) return Parent.HasComponent(serviceType);

		return false;
	}

	/// <summary>
	///     Registers the components with the <see cref="IKernel" />. The instances of <see cref="IRegistration" /> are
	///     produced by fluent registration API. Most common entry points are
	///     <see cref="Component.For{TService}" /> method to register a single type or (recommended in most cases)
	///     <see cref="Classes.FromAssembly(Assembly)" />. Let the Intellisense drive you through the
	///     fluent
	///     API past those entry points.
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
		foreach (var registration in registrations) registration.Register(this);
		if (token != null) token.Dispose();
		return this;
	}

	/// <summary>
	///     Releases a component instance. This allows the kernel to execute the proper decommission lifecycles on the
	///     component instance.
	/// </summary>
	/// <param name="instance"> </param>
	public virtual void ReleaseComponent(object instance)
	{
		if (ReleasePolicy.HasTrack(instance))
		{
			ReleasePolicy.Release(instance);
		}
		else
		{
			if (Parent != null) Parent.ReleaseComponent(instance);
		}
	}

	public virtual void RemoveChildKernel(IKernel childKernel)
	{
		ArgumentNullException.ThrowIfNull(childKernel);

		childKernel.Parent = null;
		_childKernels.Remove(childKernel);
	}

	/// <summary>
	///     Creates an implementation of <see cref="ILifestyleManager" /> based on <see cref="LifestyleType" /> and invokes
	///     <see cref="ILifestyleManager.Init" /> to initialize the newly created manager.
	/// </summary>
	/// <param name="model"> </param>
	/// <param name="activator"> </param>
	/// <returns> </returns>
	public virtual ILifestyleManager CreateLifestyleManager(ComponentModel model, IComponentActivator activator)
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
				manager = model.CustomLifestyle.CreateInstance<ILifestyleManager>();
				break;
			case LifestyleType.Pooled:
				var initial = ExtendedPropertiesConstants.PoolDefaultInitialPoolSize;
				var maxSize = ExtendedPropertiesConstants.PoolDefaultMaxPoolSize;

				if (model.ExtendedProperties.Contains(ExtendedPropertiesConstants.PoolInitialPoolSize))
					initial = (int)model.ExtendedProperties[ExtendedPropertiesConstants.PoolInitialPoolSize];
				if (model.ExtendedProperties.Contains(ExtendedPropertiesConstants.PoolMaxPoolSize))
					maxSize = (int)model.ExtendedProperties[ExtendedPropertiesConstants.PoolMaxPoolSize];

				manager = new PoolableLifestyleManager(initial, maxSize);
				break;
			default:
				//this includes LifestyleType.Undefined, LifestyleType.Singleton and invalid values
				manager = new SingletonLifestyleManager();
				break;
		}

		manager.Init(activator, this, model);

		return manager;
	}

	private static IScopeAccessor CreateScopeAccessor(ComponentModel model)
	{
		var scopeAccessorType = model.GetScopeAccessorType();
		if (scopeAccessorType == null) return new LifetimeScopeAccessor();
		return scopeAccessorType.CreateInstance<IScopeAccessor>();
	}

	private IScopeAccessor CreateScopeAccessorForBoundLifestyle(ComponentModel model)
	{
		var selector = (Func<IHandler[], IHandler>)model.ExtendedProperties[Constants.ScopeRootSelector];
		if (selector == null)
			throw new ComponentRegistrationException(
				string.Format("Component {0} has lifestyle {1} but it does not specify mandatory 'scopeRootSelector'.",
					model.Name, LifestyleType.Bound));

		return new CreationContextScopeAccessor(model, selector);
	}

	public virtual IComponentActivator CreateComponentActivator(ComponentModel model)
	{
		ArgumentNullException.ThrowIfNull(model);

		IComponentActivator activator;

		if (model.CustomComponentActivator == null)
			activator = new DefaultComponentActivator(model, this,
				RaiseComponentCreated,
				RaiseComponentDestroyed);
		else
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

		return activator;
	}

	protected CreationContext CreateCreationContext(IHandler handler, Type requestedType, Arguments additionalArguments,
		CreationContext parent,
		IReleasePolicy policy)
	{
		return new CreationContext(handler, policy, requestedType, additionalArguments, ConversionSubSystem, parent);
	}

	/// <remarks>
	///     It is the responsibility of the kernel to ensure that handler is only ever disposed once.
	/// </remarks>
	protected void DisposeHandler(IHandler handler)
	{
		if (handler is not IDisposable disposable) return;

		disposable.Dispose();
	}

	void IKernelInternal.RaiseEventsOnHandlerCreated(IHandler handler)
	{
		RaiseHandlerRegistered(handler);
		RaiseHandlersChanged();
		RaiseComponentRegistered(handler.ComponentModel.Name, handler);
	}

	protected virtual void RegisterSubSystems()
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

	protected object ResolveComponent(IHandler handler, Type service, Arguments additionalArguments,
		IReleasePolicy policy)
	{
		return ResolveComponent(handler, service, additionalArguments, policy, false);
	}

	private object ResolveComponent(IHandler handler, Type service, Arguments additionalArguments,
		IReleasePolicy policy, bool ignoreParentContext)
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

	protected virtual IHandler WrapParentHandler(IHandler parentHandler)
	{
		if (parentHandler == null) return null;

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

		for (var i = 0; i < vertices.Length; i++)
		{
			var model = (ComponentModel)vertices[i];
			var handler = NamingSubSystem.GetHandler(model.Name);
			DisposeHandler(handler);
		}
	}

	private void DisposeSubKernels()
	{
		foreach (var childKernel in _childKernels) childKernel.Dispose();
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
		if (_parentKernel == null) return;

		_parentKernel.HandlerRegistered += HandlerRegisteredOnParentKernel;
		_parentKernel.HandlersChanged += HandlersChangedOnParentKernel;
		_parentKernel.ComponentRegistered += RaiseComponentRegistered;
	}

	private void TerminateFacilities()
	{
		foreach (var facility in _facilities) facility.Terminate();
	}

	private void UnsubscribeFromParentKernel()
	{
		if (_parentKernel == null) return;

		_parentKernel.HandlerRegistered -= HandlerRegisteredOnParentKernel;
		_parentKernel.HandlersChanged -= HandlersChangedOnParentKernel;
		_parentKernel.ComponentRegistered -= RaiseComponentRegistered;
	}

	IHandler IKernelInternal.LoadHandlerByName(string name, Type service, Arguments arguments)
	{
		ArgumentNullException.ThrowIfNull(name);

		var handler = GetHandler(name);
		if (handler != null) return handler;
		lock (_lazyLoadingLock)
		{
			handler = GetHandler(name);
			if (handler != null) return handler;

			if (_isCheckingLazyLoaders) return null;

			_isCheckingLazyLoaders = true;
			try
			{
				foreach (var loader in ResolveAll<ILazyComponentLoader>())
				{
					var registration = loader.Load(name, service, arguments);
					if (registration != null)
					{
						registration.Register(this);
						return GetHandler(name);
					}
				}

				return null;
			}
			finally
			{
				_isCheckingLazyLoaders = false;
			}
		}
	}

	IHandler IKernelInternal.LoadHandlerByType(string name, Type service, Arguments arguments)
	{
		ArgumentNullException.ThrowIfNull(service);

		var handler = GetHandler(service);
		if (handler != null) return handler;

		lock (_lazyLoadingLock)
		{
			handler = GetHandler(service);
			if (handler != null) return handler;

			if (_isCheckingLazyLoaders) return null;

			_isCheckingLazyLoaders = true;
			try
			{
				foreach (var loader in ResolveAll<ILazyComponentLoader>())
				{
					var registration = loader.Load(name, service, arguments);
					if (registration != null)
					{
						registration.Register(this);
						return GetHandler(service);
					}
				}

				return null;
			}
			finally
			{
				_isCheckingLazyLoaders = false;
			}
		}
	}
}