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
using Castle.Core.Configuration;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel;
using JetBrains.Annotations;

namespace Castle.Windsor.Core;

/// <summary>Represents the collection of information and meta information collected about a component.</summary>
[Serializable]
public sealed class ComponentModel : GraphNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly ConstructorCandidateCollection _constructors = [];

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly LifecycleConcernsCollection _lifecycle = new();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly List<Type> _services = new(4);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ComponentName? _componentName;

    [NonSerialized] [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Arguments? _customDependencies;

    /// <summary>Dependencies the kernel must resolve</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private DependencyModelCollection? _dependencies;

    [NonSerialized] [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Arguments? _extendedProperties;

    /// <summary>Interceptors associated</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private InterceptorReferenceCollection? _interceptors;

    /// <summary>External parameters</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ParameterModelCollection? _parameters;

    /// <summary>All potential properties that can be setted by the kernel</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private PropertySetCollection? _properties;

    /// <summary>Constructs a ComponentModel</summary>
    public ComponentModel(ComponentName name, ICollection<Type> services, Type implementation,
        Arguments extendedProperties)
    {
        _componentName = Must.NotBeNull(name, "name");
        Implementation = Must.NotBeNull(implementation, "implementation");
        _extendedProperties = extendedProperties;
        services = Must.NotBeEmpty(services, "services");
        foreach (var type in services)
        {
            AddService(type);
        }
    }

    public ComponentModel()
    {
    }

    public ComponentName? ComponentName
    {
        get => _componentName;
        internal set => _componentName = Must.NotBeNull(value, "value");
    }

    /// <summary>Gets or sets the configuration.</summary>
    /// <value> The configuration. </value>
    public IConfiguration? Configuration { get; set; }

    /// <summary>Gets the constructors candidates.</summary>
    /// <value> The constructors. </value>
    [DebuggerDisplay("Count = {constructors.Count}")]
    public ConstructorCandidateCollection Constructors => _constructors;

    /// <summary>Gets or sets the custom component activator.</summary>
    /// <value> The custom component activator. </value>
    public Type? CustomComponentActivator { get; set; }

    /// <summary>Gets the custom dependencies.</summary>
    /// <value> The custom dependencies. </value>
    [DebuggerDisplay("Count = {customDependencies.Count}")]
    public Arguments CustomDependencies
    {
        get
        {
            var value = _customDependencies;
            if (value != null)
            {
                return value;
            }

            value = new Arguments();
            var originalValue = Interlocked.CompareExchange(ref _customDependencies, value, null);
            return originalValue ?? value;
        }
    }

    /// <summary>Gets or sets the custom lifestyle.</summary>
    /// <value> The custom lifestyle. </value>
    public Type? CustomLifestyle { get; set; }

    /// <summary>
    ///     Dependencies are kept within constructors and properties. Others dependencies must be registered here, so the
    ///     kernel (as a matter of fact the handler) can check them
    /// </summary>
    [DebuggerDisplay("Count = {dependencies.dependencies.Count}")]
    public DependencyModelCollection Dependencies
    {
        get
        {
            var value = _dependencies;
            if (value != null)
            {
                return value;
            }

            value = [];
            var originalValue = Interlocked.CompareExchange(ref _dependencies, value, null);
            return originalValue ?? value;
        }
    }

    /// <summary>Gets or sets the extended properties.</summary>
    /// <value> The extended properties. </value>
    [DebuggerDisplay("Count = {extendedProperties.Count}")]
    public Arguments ExtendedProperties
    {
        get
        {
            var value = _extendedProperties;
            if (value != null)
            {
                return value;
            }

            value = new Arguments();
            var originalValue = Interlocked.CompareExchange(ref _extendedProperties, value, null);
            return originalValue ?? value;
        }
    }

    public bool HasClassServices => _services.First().GetTypeInfo().IsClass;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool HasCustomDependencies
    {
        get
        {
            var value = _customDependencies;
            return value is { Count: > 0 };
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool HasInterceptors
    {
        get
        {
            var value = _interceptors;
            return value is { HasInterceptors: true };
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [PublicAPI]
    public bool HasParameters
    {
        get
        {
            var value = _parameters;
            return value is { Count: > 0 };
        }
    }

    /// <summary>Gets or sets the component implementation.</summary>
    /// <value> The implementation. </value>
    public Type? Implementation { get; set; }

    /// <summary>Gets or sets the strategy for inspecting public properties on the components</summary>
    public PropertiesInspectionBehavior InspectionBehavior { get; set; }

    /// <summary>Gets the interceptors.</summary>
    /// <value> The interceptors. </value>
    [DebuggerDisplay("Count = {interceptors.list.Count}")]
    public InterceptorReferenceCollection Interceptors
    {
        get
        {
            var value = _interceptors;
            if (value != null)
            {
                return value;
            }

            value = new InterceptorReferenceCollection(this);
            var originalValue = Interlocked.CompareExchange(ref _interceptors, value, null);
            return originalValue ?? value;
        }
    }

    /// <summary>Gets the lifecycle steps.</summary>
    /// <value> The lifecycle steps. </value>
    [DebuggerDisplay(
        "Count = {(lifecycle.commission != null ? lifecycle.commission.Count : 0) + (lifecycle.decommission != null ? lifecycle.decommission.Count : 0)}"
    )]
    public LifecycleConcernsCollection Lifecycle => _lifecycle;

    /// <summary>Gets or sets the lifestyle type.</summary>
    /// <value> The type of the lifestyle. </value>
    public LifestyleType LifestyleType { get; set; }

    /// <summary>Sets or returns the component key</summary>
    public string Name
    {
        get => _componentName == null
            ? throw new InvalidOperationException("Component name is not set")
            : _componentName.Name;
        set
        {
            if (_componentName == null)
            {
                throw new InvalidOperationException("Component name is not set");
            }

            _componentName.SetName(value);
        }
    }

    /// <summary>Gets the parameter collection.</summary>
    /// <value> The parameters. </value>
    public ParameterModelCollection Parameters
    {
        get
        {
            var value = _parameters;
            if (value != null)
            {
                return value;
            }

            value = [];
            var originalValue = Interlocked.CompareExchange(ref _parameters, value, null);
            return originalValue ?? value;
        }
    }

    /// <summary>Gets the properties set.</summary>
    /// <value> The properties. </value>
    [DebuggerDisplay("Count = {properties.Count}")]
    public PropertySetCollection Properties
    {
        get
        {
            var value = _properties;
            if (value != null)
            {
                return value;
            }

            value = [];
            var originalValue = Interlocked.CompareExchange(ref _properties, value, null);
            return originalValue ?? value;
        }
    }

    /// <summary>Gets or sets a value indicating whether the component requires generic arguments.</summary>
    /// <value> <c>true</c> if generic arguments are required; otherwise, <c>false</c> . </value>
    public bool RequiresGenericArguments { get; set; }

    [DebuggerDisplay("Count = {services.Count}")]
    public IEnumerable<Type> Services => _services;

    internal HashSet<Type> ServicesLookup { get; } = [];

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal ParameterModelCollection? ParametersInternal => _parameters;

    /// <summary>Adds constructor dependency to this <see cref="ComponentModel" /></summary>
    /// <param name="constructor"> </param>
    public void AddConstructor(ConstructorCandidate constructor)
    {
        (Constructors as IMutableCollection<ConstructorCandidate>).Add(constructor);
        foreach (var ctorDependency in constructor.Dependencies)
        {
            Dependencies.Add(ctorDependency);
        }
    }

    /// <summary>Adds property dependency to this <see cref="ComponentModel" /></summary>
    /// <param name="property"> </param>
    public void AddProperty(PropertySet property)
    {
        (Properties as IMutableCollection<PropertySet>).Add(property);
        Dependencies.Add(property.Dependency);
    }

    /// <summary>Add service to be exposed by this <see cref="ComponentModel" /></summary>
    /// <param name="type"> </param>
    public void AddService(Type? type)
    {
        if (type == null)
        {
            return;
        }

        if (type.IsPrimitiveType())
        {
            throw new ArgumentException(
                $"Type {type} can not be used as a service. only classes, and interfaces can be exposed as a service.");
        }

        ComponentServicesUtil.AddService(_services, ServicesLookup, type);
    }

    /// <summary>Requires the selected property dependencies.</summary>
    /// <param name="selectors"> The property selector. </param>
    public void Requires(params Predicate<PropertySet>[] selectors)
    {
        foreach (var property in Properties)
        {
            if (selectors.Any(s => s(property)))
            {
                property.Dependency.IsOptional = false;
            }
        }
    }

    /// <summary>Requires the property dependencies of type <typeparamref name="TD" /> .</summary>
    /// <typeparam name="TD"> The dependency type. </typeparam>
    public void Requires<TD>() where TD : class
    {
        Requires(p => p.Dependency.TargetItemType == typeof(TD));
    }

    public override string ToString()
    {
        var services = Services.ToArray();
        if (services.Length == 1 && services[0] == Implementation)
        {
            return Implementation.ToCSharpString();
        }

        string value;
        if (Implementation == typeof(LateBoundComponent))
        {
            value = $"late bound {services[0].ToCSharpString()}";
        }
        else if (Implementation == null)
        {
            value = "no impl / " + services[0].ToCSharpString();
        }
        else
        {
            value = $"{Implementation.ToCSharpString()} / {services[0].ToCSharpString()}";
        }

        if (services.Length > 1)
        {
            value += $" and {services.Length - 1} other services";
        }

        return value;
    }
}