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


using System;
using System.Threading.Tasks;
using Castle.MicroKernel.Lifestyle;
using Microsoft.AspNetCore.Http;

namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class AnyComponent
{
}

public class AnyComponentWithLifestyleManager : AbstractLifestyleManager
{
	public override void Dispose()
	{
	}
}

public sealed class AnyMiddleware : IMiddleware
{
	private readonly AnyComponent _anyComponent;
	private readonly CrossWiredScopedDisposable _crossWiredScopedDisposable;
	private readonly ServiceProviderOnlyScopedDisposable _serviceProviderOnlyScopedDisposable;
	private readonly WindsorOnlyScopedDisposable _windsorOnlyScopedDisposable;

	public AnyMiddleware(
		ServiceProviderOnlyScopedDisposable serviceProviderOnlyScopedDisposable,
		WindsorOnlyScopedDisposable windsorOnlyScopedDisposable,
		CrossWiredScopedDisposable crossWiredScopedDisposable)
	{
		_serviceProviderOnlyScopedDisposable = serviceProviderOnlyScopedDisposable ??
		                                       throw new ArgumentNullException(
			                                       nameof(serviceProviderOnlyScopedDisposable));
		_windsorOnlyScopedDisposable = windsorOnlyScopedDisposable ??
		                               throw new ArgumentNullException(nameof(windsorOnlyScopedDisposable));
		_crossWiredScopedDisposable = crossWiredScopedDisposable ??
		                              throw new ArgumentNullException(nameof(crossWiredScopedDisposable));
	}

	public AnyMiddleware(AnyComponent anyComponent)
	{
		// This will never get called because Windsor picks the most greedy constructor
		_anyComponent = anyComponent ?? throw new ArgumentNullException(nameof(anyComponent));
	}

	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		// Do something before
		await next(context);
		// Do something after
	}
}