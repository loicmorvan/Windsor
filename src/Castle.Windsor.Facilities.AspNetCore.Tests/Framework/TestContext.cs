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

using Castle.Windsor.Windsor;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Castle.Windsor.Facilities.AspNetCore.Tests.Framework;

public sealed class TestContext(
    IServiceCollection serviceCollection,
    IServiceProvider serviceProvider,
    IApplicationBuilder applicationBuilder,
    IWindsorContainer container,
    IDisposable windsorScope)
    : IDisposable
{
    public IApplicationBuilder ApplicationBuilder { get; } = applicationBuilder;
    public IServiceCollection ServiceCollection { get; } = serviceCollection;
    public IServiceProvider? ServiceProvider { get; private set; } = serviceProvider;

    private IDisposable? WindsorScope { get; set; } = windsorScope;
    public IWindsorContainer? WindsorContainer { get; private set; } = container;

    public void Dispose()
    {
        WindsorScope?.Dispose();
        WindsorContainer?.Dispose();
        (ServiceProvider as IDisposable)?.Dispose();
    }

    public void DisposeServiceProvider()
    {
        (ServiceProvider as IDisposable)?.Dispose();
        ServiceProvider = null;
    }

    public void DisposeWindsorContainer()
    {
        WindsorContainer?.Dispose();
        WindsorContainer = null;
    }

    public void DisposeWindsorScope()
    {
        WindsorScope?.Dispose();
        WindsorScope = null;
    }
}