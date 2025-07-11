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


using System;
using System.Diagnostics;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Extensions.DependencyInjection.Extensions;
using Castle.Windsor.Extensions.DependencyInjection.Resolvers;
using Castle.Windsor.Extensions.DependencyInjection.Scope;
using Castle.Windsor.Extensions.DependencyInjection.SubSystems;
using Microsoft.Extensions.DependencyInjection;

namespace Castle.Windsor.Extensions.DependencyInjection;

public abstract class WindsorServiceProviderFactoryBase : IServiceProviderFactory<IWindsorContainer>
{
	protected IWindsorContainer RootContainer;
	internal ExtensionContainerRootScope RootScope;

	public virtual IWindsorContainer Container => RootContainer;

	public virtual IWindsorContainer CreateBuilder(IServiceCollection services)
	{
		return BuildContainer(services, RootContainer);
	}

	public virtual IServiceProvider CreateServiceProvider(IWindsorContainer container)
	{
		return container.Resolve<IServiceProvider>();
	}

	protected virtual void CreateRootScope()
	{
		RootScope = ExtensionContainerRootScope.BeginRootScope();
	}

	protected virtual void CreateRootContainer()
	{
		SetRootContainer(new WindsorContainer());
	}

	protected virtual void SetRootContainer(IWindsorContainer container)
	{
		RootContainer = container;
		AddSubSystemToContainer(RootContainer);
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
		if (RootContainer == null) CreateRootContainer();
		Debug.Assert(RootContainer != null);

		if (serviceCollection == null) return RootContainer;

		RegisterContainer(RootContainer);
		RegisterProviders(RootContainer);
		RegisterFactories(RootContainer);

		AddSubResolvers();

		RegisterServiceCollection(serviceCollection, windsorContainer);

		return RootContainer;
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
				.DependsOn(Dependency.OnValue<ExtensionContainerRootScope>(RootScope))
				.LifestyleSingleton(),
			Component
				.For<IServiceProviderFactory<IWindsorContainer>>()
				.Instance(this)
				.LifestyleSingleton());
	}

	protected virtual void RegisterServiceCollection(IServiceCollection serviceCollection, IWindsorContainer container)
	{
		foreach (var service in serviceCollection) RootContainer.Register(service.CreateWindsorRegistration());
	}

	protected virtual void AddSubResolvers()
	{
		RootContainer.Kernel.Resolver.AddSubResolver(new RegisteredCollectionResolver(RootContainer.Kernel));
		RootContainer.Kernel.Resolver.AddSubResolver(new OptionsSubResolver(RootContainer.Kernel));
		RootContainer.Kernel.Resolver.AddSubResolver(new LoggerDependencyResolver(RootContainer.Kernel));
	}
}