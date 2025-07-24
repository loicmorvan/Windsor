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

using System;
using System.Linq;
using System.Reflection;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Context;

namespace Castle.Windsor.MicroKernel.Lifestyle.Scoped;

public class CreationContextScopeAccessor : IScopeAccessor
{
	private const string ScopeStash = "castle.scope-stash";
	private readonly ComponentModel _componentModel;
	private readonly Func<IHandler[], IHandler> _scopeRootSelector;

	public CreationContextScopeAccessor(ComponentModel componentModel, Func<IHandler[], IHandler> scopeRootSelector)
	{
		_componentModel = componentModel;
		_scopeRootSelector = scopeRootSelector;
	}

	public void Dispose()
	{
	}

	public ILifetimeScope GetScope(CreationContext context)
	{
		var selected = context.SelectScopeRoot(_scopeRootSelector);
		if (selected == null)
			throw new InvalidOperationException(
				$"Scope was not available for '{_componentModel.Name}'. No component higher up in the resolution stack met the criteria specified for scoping the component. This usually indicates a bug in custom scope root selector or means that the component is being resolved in a unforseen context (a.k.a - it's probably a bug in how the dependencies in the application are wired).");
		var stash = (DefaultLifetimeScope)selected.GetContextualProperty(ScopeStash);
		if (stash is not null) return stash;

		var newStash = new DefaultLifetimeScope(new ScopeCache());
		newStash.OnAfterCreated = burden =>
		{
			if (!burden.RequiresDecommission) return;
			selected.Burden.RequiresDecommission = true;
			selected.Burden.GraphReleased += _ => newStash.Dispose();
		};
		selected.SetContextualProperty(ScopeStash, newStash);

		return newStash;
	}

	public static IHandler DefaultScopeRootSelector<TBaseForRoot>(IHandler[] resolutionStack)
	{
		return resolutionStack.FirstOrDefault(h =>
			typeof(TBaseForRoot).GetTypeInfo().IsAssignableFrom(h.ComponentModel.Implementation));
	}

	public static IHandler NearestScopeRootSelector<TBaseForRoot>(IHandler[] resolutionStack)
	{
		return resolutionStack.LastOrDefault(h =>
			typeof(TBaseForRoot).IsAssignableFrom(h.ComponentModel.Implementation));
	}
}