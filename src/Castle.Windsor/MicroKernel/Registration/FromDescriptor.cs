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

using JetBrains.Annotations;

namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>Describes the source of types to register.</summary>
public abstract class FromDescriptor : IRegistration
{
    private readonly Predicate<Type> _additionalFilters;
    private readonly IList<BasedOnDescriptor> _criterias;
    private bool _allowMultipleMatches;

    protected FromDescriptor(Predicate<Type> additionalFilters)
    {
        _additionalFilters = additionalFilters;
        _allowMultipleMatches = false;
        _criterias = new List<BasedOnDescriptor>();
    }

    void IRegistration.Register(IKernelInternal kernel)
    {
        if (_criterias.Count == 0)
        {
            return;
        }

        foreach (var type in SelectedTypes())
        foreach (var criteria in _criterias)
        {
            if (criteria.TryRegister(type, kernel) && !_allowMultipleMatches)
            {
                break;
            }
        }
    }

    protected abstract IEnumerable<Type> SelectedTypes();

    /// <summary>Allows a type to be registered multiple times.</summary>
    public FromDescriptor AllowMultipleMatches()
    {
        _allowMultipleMatches = true;
        return this;
    }

    /// <summary>Returns the descriptor for accepting a type.</summary>
    /// <typeparam name="T"> The base type. </typeparam>
    /// <returns> The descriptor for the type. </returns>
    public BasedOnDescriptor BasedOn<T>()
    {
        return BasedOn(typeof(T));
    }

    /// <summary>Returns the descriptor for accepting a type.</summary>
    /// <param name="basedOn"> The base type. </param>
    /// <returns> The descriptor for the type. </returns>
    public BasedOnDescriptor BasedOn(Type basedOn)
    {
        return BasedOn((IEnumerable<Type>) [basedOn]);
    }

    /// <summary>Returns the descriptor for accepting a type.</summary>
    /// <param name="basedOn">
    ///     One or more base types. To be accepted a type must implement at least one of the given base
    ///     types.
    /// </param>
    /// <returns> The descriptor for the type. </returns>
    [PublicAPI]
    public BasedOnDescriptor BasedOn(params Type[] basedOn)
    {
        return BasedOn((IEnumerable<Type>)basedOn);
    }

    /// <summary>Returns the descriptor for accepting a type.</summary>
    /// <param name="basedOn">
    ///     One or more base types. To be accepted a type must implement at least one of the given base
    ///     types.
    /// </param>
    /// <returns> The descriptor for the type. </returns>
    [PublicAPI]
    public BasedOnDescriptor BasedOn(IEnumerable<Type> basedOn)
    {
        var descriptor = new BasedOnDescriptor(basedOn, this, _additionalFilters);
        _criterias.Add(descriptor);
        return descriptor;
    }

    /// <summary>Creates a predicate to check if a component is in a namespace.</summary>
    /// <param name="namespace"> The namespace. </param>
    /// <returns> true if the component type is in the namespace. </returns>
    public BasedOnDescriptor InNamespace(string @namespace)
    {
        return Where(Component.IsInNamespace(@namespace));
    }

    /// <summary>Creates a predicate to check if a component is in a namespace.</summary>
    /// <param name="namespace"> The namespace. </param>
    /// <param name="includeSubnamespaces"> If set to true, will also include types from subnamespaces. </param>
    /// <returns> true if the component type is in the namespace. </returns>
    public BasedOnDescriptor InNamespace(string @namespace, bool includeSubnamespaces)
    {
        return Where(Component.IsInNamespace(@namespace, includeSubnamespaces));
    }

    /// <summary>Creates a predicate to check if a component shares a namespace with another.</summary>
    /// <param name="type"> The component type to test namespace against. </param>
    /// <returns> true if the component is in the same namespace. </returns>
    [PublicAPI]
    public BasedOnDescriptor InSameNamespaceAs(Type type)
    {
        return Where(Component.IsInSameNamespaceAs(type));
    }

    /// <summary>Creates a predicate to check if a component shares a namespace with another.</summary>
    /// <param name="type"> The component type to test namespace against. </param>
    /// <param name="includeSubnamespaces"> If set to true, will also include types from subnamespaces. </param>
    /// <returns> true if the component is in the same namespace. </returns>
    [PublicAPI]
    public BasedOnDescriptor InSameNamespaceAs(Type type, bool includeSubnamespaces)
    {
        return Where(Component.IsInSameNamespaceAs(type, includeSubnamespaces));
    }

    /// <summary>Creates a predicate to check if a component shares a namespace with another.</summary>
    /// <typeparam name="T"> The component type to test namespace against. </typeparam>
    /// <returns> true if the component is in the same namespace. </returns>
    public BasedOnDescriptor InSameNamespaceAs<T>()
    {
        return Where(Component.IsInSameNamespaceAs<T>());
    }

    /// <summary>Creates a predicate to check if a component shares a namespace with another.</summary>
    /// <typeparam name="T"> The component type to test namespace against. </typeparam>
    /// <param name="includeSubnamespaces"> If set to true, will also include types from subnamespaces. </param>
    /// <returns> true if the component is in the same namespace. </returns>
    public BasedOnDescriptor InSameNamespaceAs<T>(bool includeSubnamespaces) where T : class
    {
        return Where(Component.IsInSameNamespaceAs<T>(includeSubnamespaces));
    }

    /// <summary>Returns the descriptor for accepting any type from given solutions.</summary>
    /// <returns> </returns>
    public BasedOnDescriptor Pick()
    {
        return BasedOn<object>();
    }

    /// <summary>Returns the descriptor for accepting a type based on a condition.</summary>
    /// <param name="accepted"> The accepting condition. </param>
    /// <returns> The descriptor for the type. </returns>
    public BasedOnDescriptor Where(Predicate<Type> accepted)
    {
        var descriptor = new BasedOnDescriptor([typeof(object)], this, _additionalFilters).If(accepted);
        _criterias.Add(descriptor);
        return descriptor;
    }
}