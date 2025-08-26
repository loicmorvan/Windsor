// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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
using System.Diagnostics.CodeAnalysis;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Castle.Windsor.Facilities.AspNetCore.Resolvers;

public class FrameworkDependencyResolver(IServiceCollection serviceCollection)
    : ISubDependencyResolver, IAcceptServiceProvider
{
    private IServiceProvider? _serviceProvider;

    public void AcceptServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
        DependencyModel dependency)
    {
        return HasMatchingType(dependency.TargetType);
    }

    public object? Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
        DependencyModel dependency)
    {
        ThrowIfServiceProviderIsNull();
        Debug.Assert(dependency.TargetType != null);
        return _serviceProvider.GetService(dependency.TargetType);
    }

    public bool HasMatchingType(Type? dependencyType)
    {
        return dependencyType != null &&
               serviceCollection.Any(x => x.ServiceType.MatchesType(dependencyType));
    }

    [MemberNotNull(nameof(_serviceProvider))]
    private void ThrowIfServiceProviderIsNull()
    {
        if (_serviceProvider == null)
        {
            throw new InvalidOperationException(
                "The serviceProvider for this resolver is null. Please call AcceptServiceProvider first.");
        }
    }
}