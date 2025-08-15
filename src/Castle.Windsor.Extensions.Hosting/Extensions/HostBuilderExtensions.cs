// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	 http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// ReSharper disable once CheckNamespace : Microsoft namespace is intentional - suggested by Microsoft
// ReSharper disable UnusedType.Global : TODO missing tests
// ReSharper disable UnusedMember.Global : TODO missing tests

using System.Diagnostics;
using Castle.Windsor.Extensions.DependencyInjection;
using Castle.Windsor.Windsor;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

public static class HostBuilderExtensions
{
    /// <summary>Uses <see name="IWindsorContainer" /> as the DI container for the host</summary>
    /// <param name="hostBuilder">Host builder</param>
    /// <returns>Host builder</returns>
    public static IHostBuilder UseWindsorContainerServiceProvider(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseServiceProviderFactory(new WindsorServiceProviderFactory());
    }

    /// <summary>Uses <see name="IWindsorContainer" /> as the DI container for the host</summary>
    /// <param name="hostBuilder">Host builder</param>
    /// <param name="factoryArgs">Ctor arguments for the creation of the factory</param>
    /// <returns>Host builder, creates instance of factory passed as generic</returns>
    public static IHostBuilder UseWindsorContainerServiceProvider<T>(this IHostBuilder hostBuilder,
        params object[] factoryArgs)
        where T : WindsorServiceProviderFactoryBase
    {
        var instance = (T)Activator.CreateInstance(typeof(T), factoryArgs);
        Debug.Assert(instance != null);
        return hostBuilder.UseServiceProviderFactory(instance);
    }

    /// <summary>Uses <see name="IWindsorContainer" /> as the DI container for the host</summary>
    /// <param name="hostBuilder">Host builder</param>
    /// <param name="serviceProviderFactory">Instance of WindsorServiceProviderFactoryBase to be used as ServiceProviderFactory</param>
    /// <returns>Host builder</returns>
    public static IHostBuilder UseWindsorContainerServiceProvider<T>(this IHostBuilder hostBuilder,
        T serviceProviderFactory)
        where T : WindsorServiceProviderFactoryBase
    {
        return hostBuilder.UseServiceProviderFactory(serviceProviderFactory);
    }

    /// <summary>Uses <see name="IWindsorContainer" /> as the DI container for the host</summary>
    /// <param name="hostBuilder">Host builder</param>
    /// <param name="container">
    ///     Windsor Container to be used for registrations, please note, will be cleared of all existing
    ///     registrations
    /// </param>
    /// <returns>Host builder</returns>
    public static IHostBuilder UseWindsorContainerServiceProvider(this IHostBuilder hostBuilder,
        IWindsorContainer container)
    {
        return hostBuilder.UseServiceProviderFactory(new WindsorServiceProviderFactory(container));
    }
}