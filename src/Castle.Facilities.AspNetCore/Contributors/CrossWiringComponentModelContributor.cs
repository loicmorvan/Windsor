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
using Castle.MicroKernel.LifecycleConcerns;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.ModelBuilder;
using Microsoft.Extensions.DependencyInjection;

namespace Castle.Facilities.AspNetCore.Contributors;

public class CrossWiringComponentModelContributor(IServiceCollection services) : IContributeComponentModelConstruction
{
	public IServiceCollection Services { get; } = services ?? throw new InvalidOperationException(
		"Please call `Container.AddFacility<AspNetCoreFacility>(f => f.CrossWiresInto(services));` first. This should happen before any cross wiring registration. Please see https://github.com/castleproject/Windsor/blob/master/docs/aspnetcore-facility.md");

	public void ProcessModel(IKernel kernel, ComponentModel model)
	{
		if (model.Configuration.Attributes.Get(AspNetCoreFacility.IsCrossWiredIntoServiceCollectionKey) ==
		    bool.TrueString)
		{
			if (model.Lifecycle.HasDecommissionConcerns)
			{
				var disposableConcern = model.Lifecycle.DecommissionConcerns.OfType<DisposalConcern>().FirstOrDefault();
				if (disposableConcern != null) model.Lifecycle.Remove(disposableConcern);
			}

			var key = model.Name;

			foreach (var serviceType in model.Services)
				if (model.LifestyleType == LifestyleType.Transient)
					Services.AddTransient(serviceType, _ => kernel.Resolve(key, serviceType));
				else if (model.LifestyleType == LifestyleType.Scoped)
					Services.AddScoped(serviceType, _ =>
					{
						kernel.RequireScope();
						return kernel.Resolve(key, serviceType);
					});
				else if (model.LifestyleType == LifestyleType.Singleton)
					Services.AddSingleton(serviceType, _ => kernel.Resolve(key, serviceType));
				else
					throw new NotSupportedException(
						$"The Castle Windsor ASP.NET Core facility only supports the following lifestyles: {nameof(LifestyleType.Transient)}, {nameof(LifestyleType.Scoped)} and {nameof(LifestyleType.Singleton)}.");
		}
	}
}