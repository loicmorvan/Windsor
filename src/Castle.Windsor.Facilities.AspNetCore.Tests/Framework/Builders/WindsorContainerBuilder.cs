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

using Castle.Windsor.Facilities.AspNetCore.Tests.Fakes;
using Castle.Windsor.Windsor;
using Microsoft.Extensions.DependencyInjection;

namespace Castle.Windsor.Facilities.AspNetCore.Tests.Framework.Builders;

public static class WindsorContainerBuilder
{
    public static IWindsorContainer New(IServiceCollection services, Action<WindsorRegistrationOptions> configure,
        Func<IServiceProvider> serviceProviderFactory)
    {
        return BuildWindsorContainer(services, configure, serviceProviderFactory);
    }

    private static IWindsorContainer BuildWindsorContainer(
        IServiceCollection services,
        Action<WindsorRegistrationOptions>? configure = null,
        Func<IServiceProvider>? serviceProviderFactory = null)
    {
        var container = new WindsorContainer().AddFacility<AspNetCoreFacility>(f => f.CrossWiresInto(services));

        RegisterApplicationComponents(container);

        services.AddWindsor(container, configure, serviceProviderFactory);

        return container;
    }

    private static void RegisterApplicationComponents(IWindsorContainer container)
    {
        ModelInstaller.RegisterWindsor(container);
        ModelInstaller.RegisterCrossWired(container);
    }
}