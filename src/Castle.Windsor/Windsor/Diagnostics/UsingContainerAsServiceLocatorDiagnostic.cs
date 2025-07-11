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
using System.Linq;
using Castle.Core.Internal;
using Castle.DynamicProxy;
using Castle.MicroKernel;
using Castle.MicroKernel.Internal;
using Castle.MicroKernel.Resolvers;

namespace Castle.Windsor.Diagnostics;

public class UsingContainerAsServiceLocatorDiagnostic(IKernel kernel) : IUsingContainerAsServiceLocatorDiagnostic
{
	public static readonly Type[] ContainerTypes =
	[
		typeof(IKernel),
		typeof(IWindsorContainer),
		typeof(IKernelEvents),
		typeof(IKernelInternal),
		typeof(DefaultKernel),
		typeof(WindsorContainer)
	];

	public static readonly Predicate<IHandler>[] ExceptionsToTheRule =
	[
		h => h.ComponentModel.Implementation.Is<IInterceptor>(),
		h => h.ComponentModel.Services.Any(s => s.Is<ILazyComponentLoader>()),
		h => h.ComponentModel.Implementation == typeof(LazyEx<>)
	];

	public IHandler[] Inspect()
	{
		var allHandlers = kernel.GetAssignableHandlers(typeof(object));
		var handlersWithContainerDependency = allHandlers.Where(HasDependencyOnTheContainer);
		return handlersWithContainerDependency
			.Where(h => ExceptionsToTheRule.Any(e => e(h)) == false)
			.ToArray();
	}

	private bool HasDependencyOnTheContainer(IHandler handler)
	{
		return handler.ComponentModel.Dependencies.Any(d => ContainerTypes.Any(c => c == d.TargetItemType));
	}
}