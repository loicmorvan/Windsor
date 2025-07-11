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
using Castle.MicroKernel.Registration;
using Castle.Windsor.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace Castle.Windsor.Extensions.DependencyInjection;

// ReSharper disable once ClassNeverInstantiated.Global
internal class RegistrationAdapter
{
	public static IRegistration FromOpenGenericServiceDescriptor(ServiceDescriptor service)
	{
		var registration = Component.For(service.ServiceType)
			.NamedAutomatically(UniqueComponentName(service));

		if (service.ImplementationType != null)
			registration = UsingImplementation(registration, service);
		else
			throw new ArgumentException("Unsupported ServiceDescriptor");

		return ResolveLifestyle(registration, service)
			.IsDefault();
	}

	public static IRegistration FromServiceDescriptor(ServiceDescriptor service)
	{
		var registration = Component.For(service.ServiceType)
			.NamedAutomatically(UniqueComponentName(service));

		if (service.ImplementationFactory != null)
			registration = UsingFactoryMethod(registration, service);
		else if (service.ImplementationInstance != null)
			registration = UsingInstance(registration, service);
		else if (service.ImplementationType != null) registration = UsingImplementation(registration, service);

		return ResolveLifestyle(registration, service)
			.IsDefault();
	}

	public static string OriginalComponentName(string uniqueComponentName)
	{
		if (uniqueComponentName == null) return null;
		if (!uniqueComponentName.Contains("@")) return uniqueComponentName;
		return uniqueComponentName.Split('@')[0];
	}

	private static string UniqueComponentName(ServiceDescriptor service)
	{
		string result;
		if (service.ImplementationType != null)
		{
			result = service.ImplementationType.FullName;
		}
		else if (service.ImplementationInstance != null)
		{
			result = service.ImplementationInstance.GetType().FullName;
		}
		else
		{
			Debug.Assert(service.ImplementationFactory != null);
			result = service.ImplementationFactory.GetType().FullName;
		}

		result = result + "@" + Guid.NewGuid();

		return result;
	}

	private static ComponentRegistration<TService> UsingFactoryMethod<TService>(
		ComponentRegistration<TService> registration, ServiceDescriptor service) where TService : class
	{
		return registration.UsingFactoryMethod(kernel =>
		{
			var serviceProvider = kernel.Resolve<IServiceProvider>();
			Debug.Assert(service.ImplementationFactory != null);
			return service.ImplementationFactory(serviceProvider) as TService;
		});
	}

	private static ComponentRegistration<TService> UsingInstance<TService>(ComponentRegistration<TService> registration,
		ServiceDescriptor service) where TService : class
	{
		return registration.Instance(service.ImplementationInstance as TService);
	}

	private static ComponentRegistration<TService> UsingImplementation<TService>(
		ComponentRegistration<TService> registration, ServiceDescriptor service) where TService : class
	{
		return registration.ImplementedBy(service.ImplementationType);
	}

	private static ComponentRegistration<TService> ResolveLifestyle<TService>(
		ComponentRegistration<TService> registration, ServiceDescriptor service) where TService : class
	{
		switch (service.Lifetime)
		{
			case ServiceLifetime.Singleton:
				return registration.LifeStyle.NetStatic();
			case ServiceLifetime.Scoped:
				return registration.LifeStyle.ScopedToNetServiceScope();
			case ServiceLifetime.Transient:
				return registration.LifestyleNetTransient();

			default:
				throw new ArgumentException($"Invalid lifetime {service.Lifetime}");
		}
	}
}