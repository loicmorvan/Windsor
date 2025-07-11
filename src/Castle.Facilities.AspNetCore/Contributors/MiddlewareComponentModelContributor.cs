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
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.ModelBuilder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Castle.Facilities.AspNetCore.Contributors;

public class MiddlewareComponentModelContributor(IServiceCollection services, IApplicationBuilder applicationBuilder)
	: IContributeComponentModelConstruction
{
	private readonly IApplicationBuilder _applicationBuilder = applicationBuilder ??
	                                                           throw new InvalidOperationException(
		                                                           "Please call `Container.GetFacility<AspNetCoreFacility>(f => f.RegistersMiddlewareInto(applicationBuilder));` first. This should happen before any middleware registration. Please see https://github.com/castleproject/Windsor/blob/master/docs/aspnetcore-facility.md");

	private readonly IServiceCollection _services = services ?? throw new ArgumentNullException(nameof(services));
	private IServiceProvider _provider;

	public void ProcessModel(IKernel kernel, ComponentModel model)
	{
		if (model.Configuration.Attributes.Get(AspNetCoreFacility.IsRegisteredAsMiddlewareIntoApplicationBuilderKey) ==
		    bool.TrueString)
			foreach (var service in model.Services)
				_applicationBuilder.Use(async (context, next) =>
				{
					var windsorScope = kernel.BeginScope();
					var serviceProviderScope = (_provider ??= _services.BuildServiceProvider()).CreateScope();
					try
					{
						var middleware = (IMiddleware)kernel.Resolve(service);
						await middleware.InvokeAsync(context, async _ => await next());
					}
					finally
					{
						serviceProviderScope.Dispose();
						windsorScope.Dispose();
					}
				});
	}
}