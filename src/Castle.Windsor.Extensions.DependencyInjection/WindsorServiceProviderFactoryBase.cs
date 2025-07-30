// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
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
using Castle.Windsor.Extensions.DependencyInjection.Extensions;
using Castle.Windsor.Extensions.DependencyInjection.Resolvers;
using Castle.Windsor.Extensions.DependencyInjection.Scope;
using Castle.Windsor.Extensions.DependencyInjection.SubSystems;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Windsor;
using Microsoft.Extensions.DependencyInjection;

namespace Castle.Windsor.Extensions.DependencyInjection;

public abstract class WindsorServiceProviderFactoryBase : IServiceProviderFactory<IWindsorContainer>
{
    private IWindsorContainer _rootContainer;
    private ExtensionContainerRootScope _rootScope;

    public virtual IWindsorContainer Container => _rootContainer;

    public virtual IWindsorContainer CreateBuilder(IServiceCollection services)
    {
        return BuildContainer(services, _rootContainer);
    }

    public virtual IServiceProvider CreateServiceProvider(IWindsorContainer container)
    {
        return container.Resolve<IServiceProvider>();
    }

    protected virtual void CreateRootScope()
    {
        _rootScope = ExtensionContainerRootScope.BeginRootScope();
    }

    protected virtual void CreateRootContainer()
    {
        SetRootContainer(new WindsorContainer());
    }

    protected virtual void SetRootContainer(IWindsorContainer container)
    {
        _rootContainer = container;
        AddSubSystemToContainer(_rootContainer);
    }

    protected virtual void AddSubSystemToContainer(IWindsorContainer container)
    {
        container.Kernel.AddSubSystem(
            SubSystemConstants.NamingKey,
            new DependencyInjectionNamingSubsystem()
        );
    }

    protected virtual IWindsorContainer BuildContainer(IServiceCollection serviceCollection,
        IWindsorContainer windsorContainer)
    {
        if (_rootContainer is null)
        {
            CreateRootContainer();
        }

        Debug.Assert(_rootContainer is not null, "Could not initialize container");

        if (serviceCollection == null)
        {
            return _rootContainer;
        }

        RegisterContainer(_rootContainer);
        RegisterProviders(_rootContainer);
        RegisterFactories(_rootContainer);

        AddSubResolvers();

        RegisterServiceCollection(serviceCollection, windsorContainer);

        return _rootContainer;
    }

    protected virtual void RegisterContainer(IWindsorContainer container)
    {
        container.Register(
            Component
                .For<IWindsorContainer>()
                .Instance(container));
    }

    protected virtual void RegisterProviders(IWindsorContainer container)
    {
        container.Register(Component
            .For<IServiceProvider, ISupportRequiredService>()
            .ImplementedBy<WindsorScopedServiceProvider>()
            .LifeStyle.ScopedToNetServiceScope());
    }

    protected virtual void RegisterFactories(IWindsorContainer container)
    {
        container.Register(Component
                .For<IServiceScopeFactory>()
                .ImplementedBy<WindsorScopeFactory>()
                .DependsOn(Dependency.OnValue<ExtensionContainerRootScope>(_rootScope))
                .LifestyleSingleton(),
            Component
                .For<IServiceProviderFactory<IWindsorContainer>>()
                .Instance(this)
                .LifestyleSingleton());
    }

    protected virtual void RegisterServiceCollection(IServiceCollection serviceCollection, IWindsorContainer container)
    {
        foreach (var service in serviceCollection)
        {
            _rootContainer.Register(service.CreateWindsorRegistration());
        }
    }

    protected virtual void AddSubResolvers()
    {
        _rootContainer.Kernel.Resolver.AddSubResolver(new RegisteredCollectionResolver(_rootContainer.Kernel));
        _rootContainer.Kernel.Resolver.AddSubResolver(new OptionsSubResolver(_rootContainer.Kernel));
        _rootContainer.Kernel.Resolver.AddSubResolver(new LoggerDependencyResolver(_rootContainer.Kernel));
    }
}