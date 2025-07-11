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
using Castle.MicroKernel;

namespace Castle.Facilities.TypedFactory;

/// <summary>
///     Represents a single component to be resolved via Typed Factory
/// </summary>
public class TypedFactoryComponentResolver
{
	private readonly Type _actualSelectorType;
	protected readonly Arguments AdditionalArguments;
	protected readonly string ComponentName;
	protected readonly Type ComponentType;
	protected readonly bool FallbackToResolveByTypeIfNameNotFound;

	public TypedFactoryComponentResolver(string componentName, Type componentType, Arguments additionalArguments,
		bool fallbackToResolveByTypeIfNameNotFound, Type actualSelectorType)
	{
		if (string.IsNullOrEmpty(componentName) && componentType == null)
			throw new ArgumentNullException(nameof(componentType),
				"At least one - componentName or componentType must not be null or empty");

		ComponentType = componentType;
		ComponentName = componentName;
		AdditionalArguments = additionalArguments;
		FallbackToResolveByTypeIfNameNotFound = fallbackToResolveByTypeIfNameNotFound;
		_actualSelectorType = actualSelectorType;
	}

	/// <summary>
	///     Resolves the component(s) from given kernel.
	/// </summary>
	/// <param name="kernel"></param>
	/// <param name="scope"></param>
	/// <returns>Resolved component(s).</returns>
	public virtual object Resolve(IKernelInternal kernel, IReleasePolicy scope)
	{
		if (LoadByName(kernel))
			try
			{
				return kernel.Resolve(ComponentName, ComponentType, AdditionalArguments, scope);
			}
			catch (ComponentNotFoundException e)
			{
				if (_actualSelectorType == typeof(DefaultDelegateComponentSelector) &&
				    FallbackToResolveByTypeIfNameNotFound == false)
				{
					e.Data["breakingChangeId"] = "typedFactoryFallbackToResolveByTypeIfNameNotFound";
					e.Data["breakingChange"] =
						"This exception may have been caused by a breaking change between Windsor 2.5 and 3.0 See breakingchanges.txt for more details.";
				}

				throw;
			}

		// Ignore thread-static parent context call stack tracking. Factory-resolved components
		// are already tracked by the factory itself and should not be added as burdens just because
		// we happen to be resolving in the call stack of some random component’s constructor.

		// Specifically, act the same as we would if the timing was slightly different and we were not
		// resolving within the call stack of the random component’s constructor.
		return kernel.Resolve(ComponentType, AdditionalArguments, scope, true);
	}

	private bool LoadByName(IKernelInternal kernel)
	{
		if (ComponentName == null) return false;
		return FallbackToResolveByTypeIfNameNotFound == false ||
		       kernel.LoadHandlerByName(ComponentName, ComponentType, AdditionalArguments) != null;
	}
}