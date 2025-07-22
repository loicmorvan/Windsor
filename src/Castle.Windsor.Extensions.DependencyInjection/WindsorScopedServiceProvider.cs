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
using Castle.Windsor.Extensions.DependencyInjection.Scope;
using Castle.Windsor.Windsor;
using Microsoft.Extensions.DependencyInjection;

namespace Castle.Windsor.Extensions.DependencyInjection
{
	internal class WindsorScopedServiceProvider : IServiceProvider, ISupportRequiredService, IDisposable
	{
		private readonly IWindsorContainer _container;
		private readonly ExtensionContainerScopeBase _scope;
		private bool _disposing;

		public WindsorScopedServiceProvider(IWindsorContainer container)
		{
			_container = container;
			_scope = ExtensionContainerScopeCache.Current;
		}

		public void Dispose()
		{
			if (_scope is not ExtensionContainerRootScope) return;
			if (_disposing) return;
			_disposing = true;
			_scope?.Dispose();
			_container.Dispose();
		}

		public object GetService(Type serviceType)
		{
			using (_ = new ForcedScope(_scope))
			{
				return ResolveInstanceOrNull(serviceType, true);
			}
		}

		public object GetRequiredService(Type serviceType)
		{
			using (_ = new ForcedScope(_scope))
			{
				return ResolveInstanceOrNull(serviceType, false);
			}
		}

		private object ResolveInstanceOrNull(Type serviceType, bool isOptional)
		{
			if (_container.Kernel.HasComponent(serviceType)) return _container.Resolve(serviceType);

			if (serviceType.GetTypeInfo().IsGenericType &&
			    serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				var allObjects = _container.ResolveAll(serviceType.GenericTypeArguments[0]);
				return allObjects;
			}

			if (isOptional) return null;

			return _container.Resolve(serviceType);
		}
	}
}