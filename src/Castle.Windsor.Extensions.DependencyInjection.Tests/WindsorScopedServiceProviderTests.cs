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
using Microsoft.Extensions.DependencyInjection;

namespace Castle.Windsor.Extensions.DependencyInjection.Tests;

public class WindsorScopedServiceProviderTests : SkippableDependencyInjectionSpecificationTests
{
	protected override IServiceProvider CreateServiceProviderImpl(IServiceCollection serviceCollection)
	{
		var factory = new WindsorServiceProviderFactory();
		var container = factory.CreateBuilder(serviceCollection);
		return factory.CreateServiceProvider(container);
	}
}