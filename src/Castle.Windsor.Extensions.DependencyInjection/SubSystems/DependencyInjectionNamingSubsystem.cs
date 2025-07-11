// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
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
using System.Reflection;
using Castle.MicroKernel;
using Castle.MicroKernel.SubSystems.Naming;

namespace Castle.Windsor.Extensions.DependencyInjection.SubSystems;

/// <summary>
///     Naming subsystem based on DefaultNamingSubSystem but GetHandlers returns handlers in registration order
/// </summary>
public class DependencyInjectionNamingSubsystem : DefaultNamingSubSystem
{
	private IHandler[] GetHandlersInRegisterOrderNoLock(Type service)
	{
		var handlers = new List<IHandler>();
		foreach (var handler in Name2Handler.Values)
		{
			if (handler.Supports(service) == false) continue;

			handlers.Add(handler);
		}

		return handlers.ToArray();
	}

	public override IHandler[] GetHandlers(Type service)
	{
		ArgumentNullException.ThrowIfNull(service);
		if (Filters != null)
		{
			var filtersOpinion = GetFiltersOpinion(service);
			if (filtersOpinion != null) return filtersOpinion;
		}

		IHandler[] result;
		using var locker = Lock.ForReadingUpgradeable();
		if (HandlerListsByTypeCache.TryGetValue(service, out result)) return result;
		result = GetHandlersInRegisterOrderNoLock(service);

		locker.Upgrade();
		HandlerListsByTypeCache[service] = result;

		return result;
	}

	public override IHandler GetHandler(Type service)
	{
		ArgumentNullException.ThrowIfNull(service);
		if (Selectors != null)
		{
			var selectorsOpinion = GetSelectorsOpinion(null, service);
			if (selectorsOpinion != null) return selectorsOpinion;
		}

		if (HandlerByServiceCache.TryGetValue(service, out var handler)) return handler;

		if (service.GetTypeInfo().IsGenericType && service.GetTypeInfo().IsGenericTypeDefinition == false)
		{
			var openService = service.GetGenericTypeDefinition();
			if (HandlerByServiceCache.TryGetValue(openService, out handler) && handler.Supports(service))
				return handler;

			//use original, priority-based GetHandlers
			var handlerCandidates = base.GetHandlers(openService);
			foreach (var handlerCandidate in handlerCandidates)
				if (handlerCandidate.Supports(service))
					return handlerCandidate;
		}

		return null;
	}
}