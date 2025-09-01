// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.SubSystems.Configuration;
using Castle.Windsor.Windsor.Configuration;
using Castle.Windsor.Windsor.Diagnostics;
using Castle.Windsor.Windsor.Installer;
using Castle.Windsor.Windsor.Proxy;

namespace Castle.Windsor.Windsor;

/// <summary>Implementation of <see cref="IWindsorContainer" /> which delegates to <see cref="IKernel" /> implementation.</summary>
[Serializable]
[DebuggerDisplay("{Name,nq}")]
[DebuggerTypeProxy(typeof(KernelDebuggerProxy))]
public sealed class WindsorContainer :
    IWindsorContainer
{
    private const string CastleUnicode = "\uD83C\uDFF0";

    private static int _instanceCount;
    private readonly Dictionary<string, IWindsorContainer> _childContainers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _childContainersLocker = new();

    private IWindsorContainer? _parent;

    /// <summary>Constructs a container without any external configuration reference</summary>
    public WindsorContainer() : this(new DefaultKernel(), new DefaultComponentInstaller())
    {
    }

    /// <summary>Constructs a container using the specified <see cref="IConfigurationStore" /> implementation.</summary>
    /// <param name="store">The instance of an <see cref="IConfigurationStore" /> implementation.</param>
    public WindsorContainer(IConfigurationStore store) : this()
    {
        Kernel.ConfigurationStore = store;

        RunInstaller();
    }

    /// <summary>Constructs a container using the specified <see cref="IConfigurationInterpreter" /> implementation.</summary>
    /// <param name="interpreter">The instance of an <see cref="IConfigurationInterpreter" /> implementation.</param>
    public WindsorContainer(IConfigurationInterpreter interpreter) : this()
    {
        ArgumentNullException.ThrowIfNull(interpreter);
        interpreter.ProcessResource(interpreter.Source, Kernel.ConfigurationStore, Kernel);

        RunInstaller();
    }

    /// <summary>Initializes a new instance of the <see cref="WindsorContainer" /> class.</summary>
    /// <param name="interpreter">The interpreter.</param>
    /// <param name="environmentInfo">The environment info.</param>
    public WindsorContainer(IConfigurationInterpreter interpreter, IEnvironmentInfo environmentInfo) : this()
    {
        ArgumentNullException.ThrowIfNull(interpreter);
        ArgumentNullException.ThrowIfNull(environmentInfo);

        interpreter.EnvironmentName = environmentInfo.GetEnvironmentName();
        interpreter.ProcessResource(interpreter.Source, Kernel.ConfigurationStore, Kernel);

        RunInstaller();
    }

    /// <summary>Constructs a container using the specified <see cref="IKernel" /> implementation. Rarely used.</summary>
    /// <remarks>This constructs sets the Kernel.ProxyFactory property to <c>Proxy.DefaultProxyFactory</c></remarks>
    /// <param name="kernel">Kernel instance</param>
    /// <param name="installer">Installer instance</param>
    public WindsorContainer(IKernel kernel, IComponentsInstaller installer) : this(MakeUniqueName(), kernel, installer)
    {
    }

    /// <summary>Constructs a container using the specified <see cref="IKernel" /> implementation. Rarely used.</summary>
    /// <remarks>This constructs sets the Kernel.ProxyFactory property to <c>Proxy.DefaultProxyFactory</c></remarks>
    /// <param name="name">Container's name</param>
    /// <param name="kernel">Kernel instance</param>
    /// <param name="installer">Installer instance</param>
    public WindsorContainer(string name, IKernel kernel, IComponentsInstaller installer)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(kernel);
        ArgumentNullException.ThrowIfNull(installer);

        Name = name;
        Kernel = kernel;
        Kernel.ProxyFactory = new DefaultProxyFactory();
        Installer = installer;
    }

    /// <summary>Constructs with a given <see cref="IProxyFactory" />.</summary>
    /// <param name="proxyFactory">A instance of an <see cref="IProxyFactory" />.</param>
    public WindsorContainer(IProxyFactory proxyFactory)
    {
        ArgumentNullException.ThrowIfNull(proxyFactory);

        Kernel = new DefaultKernel(proxyFactory);

        Installer = new DefaultComponentInstaller();
    }

    /// <summary>Constructs a container assigning a parent container before starting the dependency resolution.</summary>
    /// <param name="parent">The instance of an <see cref="IWindsorContainer" /></param>
    /// <param name="interpreter">The instance of an <see cref="IConfigurationInterpreter" /> implementation</param>
    public WindsorContainer(IWindsorContainer parent, IConfigurationInterpreter interpreter) : this()
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(interpreter);

        parent.AddChildContainer(this);

        interpreter.ProcessResource(interpreter.Source, Kernel.ConfigurationStore, Kernel);

        RunInstaller();
    }

    /// <summary>Initializes a new instance of the <see cref="WindsorContainer" /> class.</summary>
    /// <param name="name">The container's name.</param>
    /// <param name="parent">The parent.</param>
    /// <param name="interpreter">The interpreter.</param>
    public WindsorContainer(string name, IWindsorContainer parent, IConfigurationInterpreter interpreter) : this()
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(interpreter);

        Name = name;

        parent.AddChildContainer(this);

        interpreter.ProcessResource(interpreter.Source, Kernel.ConfigurationStore, Kernel);

        RunInstaller();
    }

    public IComponentsInstaller Installer { get; }

    /// <summary>Returns the inner instance of the MicroKernel</summary>
    public IKernel Kernel { get; }

    /// <summary>Gets the container's name</summary>
    /// <remarks>Only useful when child containers are being used</remarks>
    /// <value>The container's name.</value>
    public string? Name { get; }

    /// <summary>Gets or sets the parent container if this instance is a sub container.</summary>
    public IWindsorContainer? Parent
    {
        get => _parent;
        set
        {
            if (value == null)
            {
                if (_parent == null)
                {
                    return;
                }

                _parent.RemoveChildContainer(this);
                _parent = null;
            }
            else
            {
                if (value == _parent)
                {
                    return;
                }

                _parent = value;
                _parent.AddChildContainer(this);
            }
        }
    }

    /// <summary>Executes Dispose on underlying <see cref="IKernel" /></summary>
    public void Dispose()
    {
        Parent = null;
        _childContainers.Clear();
        Kernel.Dispose();
    }

    /// <summary>Registers a subcontainer. The components exposed by this container will be accessible from subcontainers.</summary>
    /// <param name="childContainer"></param>
    public void AddChildContainer(IWindsorContainer childContainer)
    {
        ArgumentNullException.ThrowIfNull(childContainer);
        ArgumentNullException.ThrowIfNull(childContainer.Name);

        lock (_childContainersLocker)
        {
            if (_childContainers.ContainsKey(childContainer.Name))
            {
                return;
            }

            Kernel.AddChildKernel(childContainer.Kernel);
            _childContainers.Add(childContainer.Name, childContainer);
            childContainer.Parent = this;
        }
    }

    /// <summary>Registers a facility within the container.</summary>
    /// <param name="facility"></param>
    public IWindsorContainer AddFacility(IFacility facility)
    {
        Kernel.AddFacility(facility);
        return this;
    }

    /// <summary>Creates and adds an <see cref="IFacility" /> facility to the container.</summary>
    /// <typeparam name="T">The facility type.</typeparam>
    /// <returns></returns>
    public IWindsorContainer AddFacility<T>() where T : IFacility, new()
    {
        Kernel.AddFacility<T>();
        return this;
    }

    /// <summary>Creates and adds an <see cref="IFacility" /> facility to the container.</summary>
    /// <typeparam name="T">The facility type.</typeparam>
    /// <param name="onCreate">The callback for creation.</param>
    /// <returns></returns>
    public IWindsorContainer AddFacility<T>(Action<T> onCreate)
        where T : IFacility, new()
    {
        Kernel.AddFacility(onCreate);
        return this;
    }

    /// <summary>Gets a child container instance by name.</summary>
    /// <param name="name">The container's name.</param>
    /// <returns>The child container instance or null</returns>
    public IWindsorContainer? GetChildContainer(string name)
    {
        lock (_childContainersLocker)
        {
            _childContainers.TryGetValue(name, out var windsorContainer);
            return windsorContainer;
        }
    }

    /// <summary>Runs the <paramref name="installers" /> so that they can register components in the container.</summary>
    /// <returns>The container.</returns>
    /// <example>
    ///     <code>
    ///     container.Install(new YourInstaller1(), new YourInstaller2(), new YourInstaller3());
    ///   </code>
    /// </example>
    /// <example>
    ///     <code>
    ///     container.Install(FromAssembly.This(), Configuration.FromAppConfig(), new SomeOtherInstaller());
    ///   </code>
    /// </example>
    public IWindsorContainer Install(params IWindsorInstaller[] installers)
    {
        ArgumentNullException.ThrowIfNull(installers);

        if (installers.Length == 0)
        {
            return this;
        }

        var scope = new DefaultComponentInstaller();

        if (Kernel is not IKernelInternal internalKernel)
        {
            Install(installers, scope);
        }
        else
        {
            var token = internalKernel.OptimizeDependencyResolution();
            Install(installers, scope);
            token?.Dispose();
        }

        return this;
    }

    /// <summary>
    ///     Registers the components with the <see cref="IWindsorContainer" />. The instances of <see cref="IRegistration" />
    ///     are produced by fluent registration API. Most common entry points are
    ///     <see cref="Component.For{TService}" /> method to register a single type or (recommended in most cases)
    ///     <see cref="Classes.FromAssembly(Assembly)" />. Let the Intellisense drive you through
    ///     the fluent API past those entry points.
    /// </summary>
    /// <example>
    ///     <code>
    ///     container.Register(Component.For&lt;IService&gt;().ImplementedBy&lt;DefaultService&gt;().LifestyleTransient());
    ///   </code>
    /// </example>
    /// <example>
    ///     <code>
    ///     container.Register(Classes.FromThisAssembly().BasedOn&lt;IService&gt;().WithServiceDefaultInterfaces().Configure(c => c.LifestyleTransient()));
    ///   </code>
    /// </example>
    /// <param name="registrations">
    ///     The component registrations created by <see cref="Component.For{TService}" />,
    ///     <see
    ///         cref="Classes.FromAssembly(Assembly)" />
    ///     or different entry method to the fluent API.
    /// </param>
    /// <returns>The container.</returns>
    public IWindsorContainer Register(params IRegistration[] registrations)
    {
        Kernel.Register(registrations);
        return this;
    }

    /// <summary>Releases a component instance</summary>
    /// <param name="instance"></param>
    public void Release(object instance)
    {
        Kernel.ReleaseComponent(instance);
    }

    /// <summary>
    ///     Removes (unregisters) a subcontainer.  The components exposed by this container will no longer be accessible
    ///     to the child container.
    /// </summary>
    /// <param name="childContainer"></param>
    public void RemoveChildContainer(IWindsorContainer childContainer)
    {
        ArgumentNullException.ThrowIfNull(childContainer);
        ArgumentNullException.ThrowIfNull(childContainer.Name);

        lock (_childContainersLocker)
        {
            if (!_childContainers.ContainsKey(childContainer.Name))
            {
                return;
            }

            Kernel.RemoveChildKernel(childContainer.Kernel);
            _childContainers.Remove(childContainer.Name);
            childContainer.Parent = null;
        }
    }

    /// <summary>Returns a component instance by the service</summary>
    /// <param name="service"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public object Resolve(Type service, Arguments arguments)
    {
        return Kernel.Resolve(service, arguments);
    }

    /// <summary>Returns a component instance by the service</summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public object Resolve(Type service)
    {
        return Kernel.Resolve(service, null);
    }

    /// <summary>Returns a component instance by the key</summary>
    /// <param name="key"></param>
    /// <param name="service"></param>
    /// <returns></returns>
    public object Resolve(string key, Type service)
    {
        return Kernel.Resolve(key, service, null);
    }

    /// <summary>Returns a component instance by the key</summary>
    /// <param name="key"></param>
    /// <param name="service"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public object Resolve(string key, Type service, Arguments arguments)
    {
        return Kernel.Resolve(key, service, arguments);
    }

    /// <summary>Returns a component instance by the service</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public T Resolve<T>(Arguments arguments)
    {
        return Kernel.Resolve<T>(arguments);
    }

    /// <summary>Returns a component instance by the key</summary>
    /// <param name="key"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public T Resolve<T>(string key, Arguments arguments)
    {
        return Kernel.Resolve<T>(key, arguments);
    }

    /// <summary>Returns a component instance by the service</summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Resolve<T>()
    {
        return (T)Kernel.Resolve(typeof(T), null);
    }

    /// <summary>Returns a component instance by the key</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Resolve<T>(string key)
    {
        return (T)Kernel.Resolve(key, typeof(T), null);
    }

    /// <summary>Resolve all valid components that match this type.</summary>
    /// <typeparam name="T">The service type</typeparam>
    public T[] ResolveAll<T>()
    {
        return (T[])ResolveAll(typeof(T));
    }

    /// <summary>Resolve all valid components that match this type.</summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public Array ResolveAll(Type service)
    {
        return Kernel.ResolveAll(service);
    }

    /// <summary>Resolve all valid components that match this type by passing dependencies as arguments.</summary>
    /// <param name="service"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public Array ResolveAll(Type service, Arguments arguments)
    {
        return Kernel.ResolveAll(service, arguments);
    }

    /// <summary>
    ///     Resolve all valid components that match this type.
    ///     <typeparam name="T">The service type</typeparam>
    ///     <param name="arguments">Arguments to resolve the service</param>
    /// </summary>
    public T[] ResolveAll<T>(Arguments arguments)
    {
        return (T[])ResolveAll(typeof(T), arguments);
    }

    private void RunInstaller()
    {
        Installer.SetUp(this, Kernel.ConfigurationStore);
    }

    private void Install(IWindsorInstaller[] installers, DefaultComponentInstaller scope)
    {
        using var store = new PartialConfigurationStore((IKernelInternal)Kernel);
        foreach (var windsorInstaller in installers)
        {
            windsorInstaller.Install(this, store);
        }

        scope.SetUp(this, store);
    }

    private static string MakeUniqueName()
    {
        var sb = new StringBuilder();
        sb.Append(CastleUnicode);
        sb.Append(' ');
        sb.Append(Interlocked.Increment(ref _instanceCount));
        return sb.ToString();
    }
}