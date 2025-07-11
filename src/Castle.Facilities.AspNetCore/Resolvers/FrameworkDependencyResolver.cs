﻿// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Castle.Facilities.AspNetCore.Resolvers;

public class FrameworkDependencyResolver(IServiceCollection serviceCollection)
	: ISubDependencyResolver, IAcceptServiceProvider
{
	private IServiceProvider _serviceProvider;

	public void AcceptServiceProvider(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
		DependencyModel dependency)
	{
		return HasMatchingType(dependency.TargetType);
	}

	public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
		DependencyModel dependency)
	{
		ThrowIfServiceProviderIsNull();
		return _serviceProvider.GetService(dependency.TargetType);
	}

	public bool HasMatchingType(Type dependencyType)
	{
		return dependencyType != null &&
		       serviceCollection.Any(x => x.ServiceType.MatchesType(dependencyType));
	}

	private void ThrowIfServiceProviderIsNull()
	{
		if (_serviceProvider == null)
			throw new InvalidOperationException(
				"The serviceProvider for this resolver is null. Please call AcceptServiceProvider first.");
	}
}

internal static class GenericTypeExtensions
{
	public static bool MatchesType(this Type type, Type otherType)
	{
		var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
		var genericOtherType = otherType.IsGenericType ? otherType.GetGenericTypeDefinition() : otherType;
		return genericType == genericOtherType;
	}
}