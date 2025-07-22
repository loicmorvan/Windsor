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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Context;
using Castle.Windsor.MicroKernel.Resolvers;

namespace Castle.Windsor.MicroKernel;

/// <summary>Reference to component obtained from the container.</summary>
/// <typeparam name = "T"></typeparam>
[Serializable]
public class ComponentReference<T> : IReference<T>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	protected readonly string ReferencedComponentName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	protected readonly Type ReferencedComponentType;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	protected DependencyModel DependencyModel;

	/// <summary>
	///     Creates a new instance of <see cref = "ComponentReference{T}" /> referencing default component implemented by
	///     <paramref
	///         name = "componentType" />
	/// </summary>
	/// <param name = "componentType"></param>
	public ComponentReference(Type componentType)
	{
		ReferencedComponentName = ComponentName.DefaultNameFor(componentType);
		ReferencedComponentType = componentType;
	}

	/// <summary>
	///     Creates a new instance of <see cref = "ComponentReference{T}" /> referencing component
	///     <paramref
	///         name = "referencedComponentName" />
	/// </summary>
	/// <param name = "referencedComponentName"></param>
	public ComponentReference(string referencedComponentName)
	{
		ArgumentNullException.ThrowIfNull(referencedComponentName);
		ReferencedComponentName = referencedComponentName;
	}

	protected virtual Type ComponentType => ReferencedComponentType ?? typeof(T);

	public T Resolve(IKernel kernel, CreationContext context)
	{
		var handler = GetHandler(kernel);
		if (handler == null)
			throw new DependencyResolverException(
				$"The referenced component {ReferencedComponentName} could not be resolved. Make sure you didn't misspell the name, and that component is registered.");

		if (handler.IsBeingResolvedInContext(context))
			throw new DependencyResolverException(
				$"Cycle detected - referenced component {handler.ComponentModel.Name} wants to use itself as its dependency. This usually signifies a bug in your code.");

		var contextForInterceptor = RebuildContext(handler, context);

		try
		{
			return (T)handler.Resolve(contextForInterceptor);
		}
		catch (InvalidCastException e)
		{
			throw new ComponentResolutionException(
				$"Component {ReferencedComponentName} is not compatible with type {typeof(T)}.", e);
		}
	}

	void IReference<T>.Attach(ComponentModel component)
	{
		DependencyModel = new ComponentDependencyModel(ReferencedComponentName, ComponentType);
		component.Dependencies.Add(DependencyModel);
	}

	void IReference<T>.Detach(ComponentModel component)
	{
		if (DependencyModel == null) return;
		component.Dependencies.Remove(DependencyModel);
	}

	private IHandler GetHandler(IKernel kernel)
	{
		var handler = kernel.GetHandler(ReferencedComponentName);
		return handler;
	}

	private CreationContext RebuildContext(IHandler handler, CreationContext current)
	{
		var handlerType = ComponentType ?? handler.ComponentModel.Services.First();
		if (handlerType.GetTypeInfo().ContainsGenericParameters) return current;

		return new CreationContext(handlerType, current, false);
	}
}