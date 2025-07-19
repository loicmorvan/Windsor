/*
 * Based upon: https://github.com/dotnet/runtime/blob/v6.0.9/src/libraries/Microsoft.Extensions.DependencyInjection/tests/DI.External.Tests/SkippableDependencyInjectionSpecificationTests.cs
 *
 * Why is this necessary?
 * ----------------------
 * There is one test (SingletonServiceCanBeResolvedFromScope - https://github.com/dotnet/runtime/blob/v6.0.9/src/libraries/Microsoft.Extensions.DependencyInjection.Specification.Tests/src/DependencyInjectionSpecificationTests.cs#L125-L155)
 * that relies on a behaviour that Windsor doesn't share with MEDI, that of Scopes being their own `IServiceProvider`.
 * When you create a new Scope in Windsor, it uses the same ServiceProvider that's passed in,
 *	whereas in MEDI, a Scope is a ServiceProvider in its own right.
 *
 * Compare:
 *	Castle.Windsor.Extensions.DependencyInjection.Scope.ServiceScope
 * with:
 *	Microsoft.Extensions.DependencyInjection.ServiceLookup.ServiceProviderEngineScope (https://github.com/dotnet/runtime/blob/v6.0.9/src/libraries/Microsoft.Extensions.DependencyInjection/src/ServiceLookup/ServiceProviderEngineScope.cs)
 *
 * Specifically, how the `ServiceProvider` property is implemented.
 */

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection.Specification;

using System;
using System.Diagnostics;
using System.Linq;

public abstract class SkippableDependencyInjectionSpecificationTests : DependencyInjectionSpecificationTests
{
	private static string[] SkippedTests => ["SingletonServiceCanBeResolvedFromScope"];

	public override bool SupportsIServiceProviderIsService => false;

	protected sealed override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
	{
		return new StackTrace(1).GetFrames().Take(2).Any(stackFrame => SkippedTests.Contains(stackFrame.GetMethod().Name))
			? serviceCollection.BuildServiceProvider()
			: CreateServiceProviderImpl(serviceCollection);
	}

	protected abstract IServiceProvider CreateServiceProviderImpl(IServiceCollection serviceCollection);
}