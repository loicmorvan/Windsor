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

// ReSharper disable UnusedTypeParameter
// ReSharper disable MemberCanBeProtected.Global
namespace Castle.Facilities.AspNetCore.Tests.Fakes;

using System;

using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Windsor;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

[UsedImplicitly]
public class OpenOptions;

[UsedImplicitly]
public class ClosedOptions;

public interface IDisposableObservable
{
	bool Disposed { get; set; }
	int DisposedCount { get; set; }
}

public interface IWeakReferenceObservable
{
	bool HasReference { get; }
}

public class ServiceProviderOnlyTransient : IWeakReferenceObservable
{
	private readonly WeakReference reference;

	public ServiceProviderOnlyTransient()
	{
		reference = new WeakReference(this, false);
	}

	public bool HasReference => reference.IsAlive;
}

public class ServiceProviderOnlyTransientGeneric<T> : ServiceProviderOnlyTransient;

public class ServiceProviderOnlyTransientDisposable : ServiceProviderOnlyTransient, IDisposable, IDisposableObservable
{
	public void Dispose()
	{
		Disposed = true;
		DisposedCount++;
	}

	public bool Disposed { get; set; }
	public int DisposedCount { get; set; }
}

public class ServiceProviderOnlyScoped : IWeakReferenceObservable
{
	private readonly WeakReference reference;

	public ServiceProviderOnlyScoped()
	{
		reference = new WeakReference(this, false);
	}

	public bool HasReference => reference.IsAlive;
}

public class ServiceProviderOnlyScopedGeneric<T> : ServiceProviderOnlyScoped;

public class ServiceProviderOnlyScopedDisposable : ServiceProviderOnlyScoped, IDisposable, IDisposableObservable
{
	public void Dispose()
	{
		Disposed = true;
		DisposedCount++;
	}

	public bool Disposed { get; set; }
	public int DisposedCount { get; set; }
}

public class ServiceProviderOnlySingleton : IWeakReferenceObservable
{
	private readonly WeakReference reference;

	public ServiceProviderOnlySingleton()
	{
		reference = new WeakReference(this, false);
	}

	public bool HasReference => reference.IsAlive;
}

public class ServiceProviderOnlySingletonGeneric<T> : ServiceProviderOnlySingleton;

public class ServiceProviderOnlySingletonDisposable : ServiceProviderOnlySingleton, IDisposable, IDisposableObservable
{
	public void Dispose()
	{
		Disposed = true;
		DisposedCount++;
	}

	public bool Disposed { get; set; }
	public int DisposedCount { get; set; }
}

public class WindsorOnlyTransient : IWeakReferenceObservable
{
	private readonly WeakReference reference;

	public WindsorOnlyTransient()
	{
		reference = new WeakReference(this, false);
	}

	public bool HasReference => reference.IsAlive;
}

public class WindsorOnlyTransientGeneric<T> : WindsorOnlyTransient;

public class WindsorOnlyTransientDisposable : WindsorOnlyTransient, IDisposable, IDisposableObservable
{
	public void Dispose()
	{
		Disposed = true;
		DisposedCount++;
	}

	public bool Disposed { get; set; }
	public int DisposedCount { get; set; }
}

public class WindsorOnlyScoped : IWeakReferenceObservable
{
	private readonly WeakReference reference;

	public WindsorOnlyScoped()
	{
		reference = new WeakReference(this, false);
	}

	public bool HasReference => reference.IsAlive;
}

public class WindsorOnlyScopedGeneric<T> : WindsorOnlyScoped;

public class WindsorOnlyScopedDisposable : WindsorOnlyScoped, IDisposable, IDisposableObservable
{
	public void Dispose()
	{
		Disposed = true;
		DisposedCount++;
	}

	public bool Disposed { get; set; }
	public int DisposedCount { get; set; }
}

public class WindsorOnlySingleton : IWeakReferenceObservable
{
	private readonly WeakReference reference;

	public WindsorOnlySingleton()
	{
		reference = new WeakReference(this, false);
	}

	public bool HasReference => reference.IsAlive;
}

public class WindsorOnlySingletonGeneric<T> : WindsorOnlySingleton;

public class WindsorOnlySingletonDisposable : WindsorOnlySingleton, IDisposable, IDisposableObservable
{
	public void Dispose()
	{
		Disposed = true;
		DisposedCount++;
	}

	public bool Disposed { get; set; }
	public int DisposedCount { get; set; }
}

public class CrossWiredTransient : IWeakReferenceObservable
{
	private readonly WeakReference reference;

	public CrossWiredTransient()
	{
		reference = new WeakReference(this, false);
	}

	public bool HasReference => reference.IsAlive;
}

public class CrossWiredTransientGeneric<T> : CrossWiredTransient;

public class CrossWiredTransientDisposable : CrossWiredTransient, IDisposable, IDisposableObservable
{
	public void Dispose()
	{
		Disposed = true;
		DisposedCount++;
	}

	public bool Disposed { get; set; }
	public int DisposedCount { get; set; }
}

public class CrossWiredScoped : IWeakReferenceObservable
{
	private readonly WeakReference reference;

	public CrossWiredScoped()
	{
		reference = new WeakReference(this, false);
	}

	public bool HasReference => reference.IsAlive;
}

public class CrossWiredScopedGeneric<T> : CrossWiredScoped;

public class CrossWiredScopedDisposable : CrossWiredScoped, IDisposable, IDisposableObservable
{
	public void Dispose()
	{
		Disposed = true;
		DisposedCount++;
	}

	public bool Disposed { get; set; }
	public int DisposedCount { get; set; }
}

public class CrossWiredSingleton : IWeakReferenceObservable
{
	private readonly WeakReference reference;

	public CrossWiredSingleton()
	{
		reference = new WeakReference(this, false);
	}

	public bool HasReference => reference.IsAlive;
}

public class CrossWiredSingletonGeneric<T> : CrossWiredSingleton;

public class CrossWiredSingletonDisposable : CrossWiredSingleton, IDisposable, IDisposableObservable
{
	public void Dispose()
	{
		Disposed = true;
		DisposedCount++;
	}

	public bool Disposed { get; set; }
	public int DisposedCount { get; set; }
}

[UsedImplicitly]
public class ModelInstaller
{
	public static void RegisterWindsor(IWindsorContainer container)
	{
		container.Register(Component.For<WindsorOnlyTransient>().LifestyleTransient());
		container.Register(Component.For(typeof(WindsorOnlyTransientGeneric<>)).LifestyleTransient());
		container.Register(Component.For<WindsorOnlyTransientGeneric<ClosedOptions>>().LifestyleTransient());
		container.Register(Component.For<WindsorOnlyTransientDisposable>().LifestyleTransient());

		container.Register(Component.For<WindsorOnlyScoped>().LifestyleScoped());
		container.Register(Component.For(typeof(WindsorOnlyScopedGeneric<>)).LifestyleScoped());
		container.Register(Component.For<WindsorOnlyScopedGeneric<ClosedOptions>>().LifestyleScoped());
		container.Register(Component.For<WindsorOnlyScopedDisposable>().LifestyleScoped());

		container.Register(Component.For<WindsorOnlySingleton>().LifestyleSingleton());
		container.Register(Component.For(typeof(WindsorOnlySingletonGeneric<>)).LifestyleSingleton());
		container.Register(Component.For<WindsorOnlySingletonGeneric<ClosedOptions>>().LifestyleSingleton());
		container.Register(Component.For<WindsorOnlySingletonDisposable>().LifestyleSingleton());

		container.Register(Component.For<ControllerWindsorOnly>().LifestyleScoped());
		container.Register(Component.For<TagHelperWindsorOnly>().LifestyleScoped());
		container.Register(Component.For<ViewComponentWindsorOnly>().LifestyleScoped());
	}

	public static void RegisterServiceCollection(IServiceCollection services)
	{
		services.AddTransient<ServiceProviderOnlyTransient>();
		services.AddTransient(typeof(ServiceProviderOnlyTransientGeneric<OpenOptions>));
		services.AddTransient<ServiceProviderOnlyTransientGeneric<ClosedOptions>>();
		services.AddTransient<ServiceProviderOnlyTransientDisposable>();

		services.AddScoped<ServiceProviderOnlyScoped>();
		services.AddScoped(typeof(ServiceProviderOnlyScopedGeneric<OpenOptions>));
		services.AddScoped<ServiceProviderOnlyScopedGeneric<ClosedOptions>>();
		services.AddScoped<ServiceProviderOnlyScopedDisposable>();

		services.AddSingleton<ServiceProviderOnlySingleton>();
		services.AddSingleton(typeof(ServiceProviderOnlySingletonGeneric<OpenOptions>));
		services.AddSingleton<ServiceProviderOnlySingletonGeneric<ClosedOptions>>();
		services.AddSingleton<ServiceProviderOnlySingletonDisposable>();

		services.AddScoped<ControllerServiceProviderOnly>();
		services.AddScoped<TagHelperServiceProviderOnly>();
		services.AddScoped<ViewComponentServiceProviderOnly>();
	}

	public static void RegisterCrossWired(IWindsorContainer container, IServiceCollection serviceCollection)
	{
		container.Register(Component.For<CrossWiredTransient>().CrossWired().LifestyleTransient());
		container.Register(Component.For<CrossWiredTransientGeneric<OpenOptions>>().CrossWired().LifestyleTransient());
		container.Register(Component.For<CrossWiredTransientGeneric<ClosedOptions>>().CrossWired().LifestyleTransient());
		container.Register(Component.For<CrossWiredTransientDisposable>().CrossWired().LifestyleTransient());

		container.Register(Component.For<CrossWiredScoped>().CrossWired().LifestyleScoped());
		container.Register(Component.For<CrossWiredScopedGeneric<OpenOptions>>().CrossWired().LifestyleScoped());
		container.Register(Component.For<CrossWiredScopedGeneric<ClosedOptions>>().CrossWired().LifestyleScoped());
		container.Register(Component.For<CrossWiredScopedDisposable>().CrossWired().LifestyleScoped());

		container.Register(Component.For<CrossWiredSingleton>().CrossWired().LifestyleSingleton());
		container.Register(Component.For<CrossWiredSingletonGeneric<OpenOptions>>().CrossWired().LifestyleSingleton());
		container.Register(Component.For<CrossWiredSingletonGeneric<ClosedOptions>>().CrossWired().LifestyleSingleton());
		container.Register(Component.For<CrossWiredSingletonDisposable>().CrossWired().LifestyleSingleton());

		container.Register(Component.For<OpenOptions>().CrossWired().LifestyleSingleton());
		container.Register(Component.For<ControllerCrossWired>().CrossWired().LifestyleScoped());
		container.Register(Component.For<TagHelperCrossWired>().CrossWired().LifestyleScoped());
		container.Register(Component.For<ViewComponentCrossWired>().CrossWired().LifestyleScoped());
	}
}

public class ControllerWindsorOnly : Controller
{
	public ControllerWindsorOnly
	(
		WindsorOnlyTransient windsorOnlyTransient1,
		WindsorOnlyTransientGeneric<OpenOptions> windsorOnlyTransient2,
		WindsorOnlyTransientGeneric<ClosedOptions> windsorOnlyTransient3,
		WindsorOnlyTransientDisposable windsorOnlyTransient4,
		WindsorOnlyScoped windsorOnlyScoped1,
		WindsorOnlyScopedGeneric<OpenOptions> windsorOnlyScoped2,
		WindsorOnlyScopedGeneric<ClosedOptions> windsorOnlyScoped3,
		WindsorOnlyScopedDisposable windsorOnlyScoped4,
		WindsorOnlySingleton windsorOnlySingleton1,
		WindsorOnlySingletonGeneric<OpenOptions> windsorOnlySingleton2,
		WindsorOnlySingletonGeneric<ClosedOptions> windsorOnlySingleton3,
		WindsorOnlySingletonDisposable windsorOnlySingleton4)
	{
		ArgumentNullException.ThrowIfNull(windsorOnlyTransient1);
		ArgumentNullException.ThrowIfNull(windsorOnlyTransient2);
		ArgumentNullException.ThrowIfNull(windsorOnlyTransient3);
		ArgumentNullException.ThrowIfNull(windsorOnlyTransient4);
		ArgumentNullException.ThrowIfNull(windsorOnlyScoped1);
		ArgumentNullException.ThrowIfNull(windsorOnlyScoped2);
		ArgumentNullException.ThrowIfNull(windsorOnlyScoped3);
		ArgumentNullException.ThrowIfNull(windsorOnlyScoped4);
		ArgumentNullException.ThrowIfNull(windsorOnlySingleton1);
		ArgumentNullException.ThrowIfNull(windsorOnlySingleton2);
		ArgumentNullException.ThrowIfNull(windsorOnlySingleton3);
		ArgumentNullException.ThrowIfNull(windsorOnlySingleton4);
	}
}

public class TagHelperWindsorOnly : TagHelper
{
	public TagHelperWindsorOnly
	(
		WindsorOnlyTransient windsorOnlyTransient1,
		WindsorOnlyTransientGeneric<OpenOptions> windsorOnlyTransient2,
		WindsorOnlyTransientGeneric<ClosedOptions> windsorOnlyTransient3,
		WindsorOnlyTransientDisposable windsorOnlyTransient4,
		WindsorOnlyScoped windsorOnlyScoped1,
		WindsorOnlyScopedGeneric<OpenOptions> windsorOnlyScoped2,
		WindsorOnlyScopedGeneric<ClosedOptions> windsorOnlyScoped3,
		WindsorOnlyScopedDisposable windsorOnlyScoped4,
		WindsorOnlySingleton windsorOnlySingleton1,
		WindsorOnlySingletonGeneric<OpenOptions> windsorOnlySingleton2,
		WindsorOnlySingletonGeneric<ClosedOptions> windsorOnlySingleton3,
		WindsorOnlySingletonDisposable windsorOnlySingleton4)
	{
		ArgumentNullException.ThrowIfNull(windsorOnlyTransient1);
		ArgumentNullException.ThrowIfNull(windsorOnlyTransient2);
		ArgumentNullException.ThrowIfNull(windsorOnlyTransient3);
		ArgumentNullException.ThrowIfNull(windsorOnlyTransient4);
		ArgumentNullException.ThrowIfNull(windsorOnlyScoped1);
		ArgumentNullException.ThrowIfNull(windsorOnlyScoped2);
		ArgumentNullException.ThrowIfNull(windsorOnlyScoped3);
		ArgumentNullException.ThrowIfNull(windsorOnlyScoped4);
		ArgumentNullException.ThrowIfNull(windsorOnlySingleton1);
		ArgumentNullException.ThrowIfNull(windsorOnlySingleton2);
		ArgumentNullException.ThrowIfNull(windsorOnlySingleton3);
		ArgumentNullException.ThrowIfNull(windsorOnlySingleton4);
	}
}

public class ViewComponentWindsorOnly : ViewComponent
{
	public ViewComponentWindsorOnly
	(
		WindsorOnlyTransient windsorOnlyTransient1,
		WindsorOnlyTransientGeneric<OpenOptions> windsorOnlyTransient2,
		WindsorOnlyTransientGeneric<ClosedOptions> windsorOnlyTransient3,
		WindsorOnlyTransientDisposable windsorOnlyTransient4,
		WindsorOnlyScoped windsorOnlyScoped1,
		WindsorOnlyScopedGeneric<OpenOptions> windsorOnlyScoped2,
		WindsorOnlyScopedGeneric<ClosedOptions> windsorOnlyScoped3,
		WindsorOnlyScopedDisposable windsorOnlyScoped4,
		WindsorOnlySingleton windsorOnlySingleton1,
		WindsorOnlySingletonGeneric<OpenOptions> windsorOnlySingleton2,
		WindsorOnlySingletonGeneric<ClosedOptions> windsorOnlySingleton3,
		WindsorOnlySingletonDisposable windsorOnlySingleton4)
	{
		ArgumentNullException.ThrowIfNull(windsorOnlyTransient1);
		ArgumentNullException.ThrowIfNull(windsorOnlyTransient2);
		ArgumentNullException.ThrowIfNull(windsorOnlyTransient3);
		ArgumentNullException.ThrowIfNull(windsorOnlyTransient4);
		ArgumentNullException.ThrowIfNull(windsorOnlyScoped1);
		ArgumentNullException.ThrowIfNull(windsorOnlyScoped2);
		ArgumentNullException.ThrowIfNull(windsorOnlyScoped3);
		ArgumentNullException.ThrowIfNull(windsorOnlyScoped4);
		ArgumentNullException.ThrowIfNull(windsorOnlySingleton1);
		ArgumentNullException.ThrowIfNull(windsorOnlySingleton2);
		ArgumentNullException.ThrowIfNull(windsorOnlySingleton3);
		ArgumentNullException.ThrowIfNull(windsorOnlySingleton4);
	}
}

public class ControllerServiceProviderOnly : Controller
{
	public ControllerServiceProviderOnly
	(
		ServiceProviderOnlyTransient serviceProviderOnlyTransient1,
		ServiceProviderOnlyTransientGeneric<OpenOptions> serviceProviderOnlyTransient2,
		ServiceProviderOnlyTransientGeneric<ClosedOptions> serviceProviderOnlyTransient3,
		ServiceProviderOnlyTransientDisposable serviceProviderOnlyTransient4,
		ServiceProviderOnlyScoped serviceProviderOnlyScoped1,
		ServiceProviderOnlyScopedGeneric<OpenOptions> serviceProviderOnlyScoped2,
		ServiceProviderOnlyScopedGeneric<ClosedOptions> serviceProviderOnlyScoped3,
		ServiceProviderOnlyScopedDisposable serviceProviderOnlyScoped4,
		ServiceProviderOnlySingleton serviceProviderOnlySingleton1,
		ServiceProviderOnlySingletonGeneric<OpenOptions> serviceProviderOnlySingleton2,
		ServiceProviderOnlySingletonGeneric<ClosedOptions> serviceProviderOnlySingleton3,
		ServiceProviderOnlySingletonDisposable serviceProviderOnlySingleton4)
	{
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyTransient1);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyTransient2);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyTransient3);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyTransient4);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyScoped1);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyScoped2);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyScoped3);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyScoped4);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlySingleton1);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlySingleton2);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlySingleton3);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlySingleton4);
	}
}

public class TagHelperServiceProviderOnly : TagHelper
{
	public TagHelperServiceProviderOnly
	(
		ServiceProviderOnlyTransient serviceProviderOnlyTransient1,
		ServiceProviderOnlyTransientGeneric<OpenOptions> serviceProviderOnlyTransient2,
		ServiceProviderOnlyTransientGeneric<ClosedOptions> serviceProviderOnlyTransient3,
		ServiceProviderOnlyTransientDisposable serviceProviderOnlyTransient4,
		ServiceProviderOnlyScoped serviceProviderOnlyScoped1,
		ServiceProviderOnlyScopedGeneric<OpenOptions> serviceProviderOnlyScoped2,
		ServiceProviderOnlyScopedGeneric<ClosedOptions> serviceProviderOnlyScoped3,
		ServiceProviderOnlyScopedDisposable serviceProviderOnlyScoped4,
		ServiceProviderOnlySingleton serviceProviderOnlySingleton1,
		ServiceProviderOnlySingletonGeneric<OpenOptions> serviceProviderOnlySingleton2,
		ServiceProviderOnlySingletonGeneric<ClosedOptions> serviceProviderOnlySingleton3,
		ServiceProviderOnlySingletonDisposable serviceProviderOnlySingleton4)
	{
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyTransient1);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyTransient2);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyTransient3);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyTransient4);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyScoped1);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyScoped2);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyScoped3);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyScoped4);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlySingleton1);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlySingleton2);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlySingleton3);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlySingleton4);
	}
}

public class ViewComponentServiceProviderOnly : ViewComponent
{
	public ViewComponentServiceProviderOnly
	(
		ServiceProviderOnlyTransient serviceProviderOnlyTransient1,
		ServiceProviderOnlyTransientGeneric<OpenOptions> serviceProviderOnlyTransient2,
		ServiceProviderOnlyTransientGeneric<ClosedOptions> serviceProviderOnlyTransient3,
		ServiceProviderOnlyTransientDisposable serviceProviderOnlyTransient4,
		ServiceProviderOnlyScoped serviceProviderOnlyScoped1,
		ServiceProviderOnlyScopedGeneric<OpenOptions> serviceProviderOnlyScoped2,
		ServiceProviderOnlyScopedGeneric<ClosedOptions> serviceProviderOnlyScoped3,
		ServiceProviderOnlyScopedDisposable serviceProviderOnlyScoped4,
		ServiceProviderOnlySingleton serviceProviderOnlySingleton1,
		ServiceProviderOnlySingletonGeneric<OpenOptions> serviceProviderOnlySingleton2,
		ServiceProviderOnlySingletonGeneric<ClosedOptions> serviceProviderOnlySingleton3,
		ServiceProviderOnlySingletonDisposable serviceProviderOnlySingleton4)
	{
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyTransient1);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyTransient2);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyTransient3);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyTransient4);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyScoped1);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyScoped2);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyScoped3);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyScoped4);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlySingleton1);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlySingleton2);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlySingleton3);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlySingleton4);
	}
}

public class ControllerCrossWired : Controller
{
	public ControllerCrossWired
	(
		CrossWiredTransient crossWiredTransient1,
		CrossWiredTransientGeneric<OpenOptions> crossWiredTransient2,
		CrossWiredTransientGeneric<ClosedOptions> crossWiredTransient3,
		CrossWiredTransientDisposable crossWiredTransient4,
		CrossWiredScoped crossWiredScoped1,
		CrossWiredScopedGeneric<OpenOptions> crossWiredScoped2,
		CrossWiredScopedGeneric<ClosedOptions> crossWiredScoped3,
		CrossWiredScopedDisposable crossWiredScoped4,
		CrossWiredSingleton crossWiredSingleton1,
		CrossWiredSingletonGeneric<OpenOptions> crossWiredSingleton2,
		CrossWiredSingletonGeneric<ClosedOptions> crossWiredSingleton3,
		CrossWiredSingletonDisposable crossWiredSingleton4)
	{
		ArgumentNullException.ThrowIfNull(crossWiredTransient1);
		ArgumentNullException.ThrowIfNull(crossWiredTransient2);
		ArgumentNullException.ThrowIfNull(crossWiredTransient3);
		ArgumentNullException.ThrowIfNull(crossWiredTransient4);
		ArgumentNullException.ThrowIfNull(crossWiredScoped1);
		ArgumentNullException.ThrowIfNull(crossWiredScoped2);
		ArgumentNullException.ThrowIfNull(crossWiredScoped3);
		ArgumentNullException.ThrowIfNull(crossWiredScoped4);
		ArgumentNullException.ThrowIfNull(crossWiredSingleton1);
		ArgumentNullException.ThrowIfNull(crossWiredSingleton2);
		ArgumentNullException.ThrowIfNull(crossWiredSingleton3);
		ArgumentNullException.ThrowIfNull(crossWiredSingleton4);
	}
}

public class TagHelperCrossWired : TagHelper
{
	public TagHelperCrossWired
	(
		CrossWiredTransient crossWiredTransient1,
		CrossWiredTransientGeneric<OpenOptions> crossWiredTransient2,
		CrossWiredTransientGeneric<ClosedOptions> crossWiredTransient3,
		CrossWiredTransientDisposable crossWiredTransient4,
		CrossWiredScoped crossWiredScoped1,
		CrossWiredScopedGeneric<OpenOptions> crossWiredScoped2,
		CrossWiredScopedGeneric<ClosedOptions> crossWiredScoped3,
		CrossWiredScopedDisposable crossWiredScoped4,
		CrossWiredSingleton crossWiredSingleton1,
		CrossWiredSingletonGeneric<OpenOptions> crossWiredSingleton2,
		CrossWiredSingletonGeneric<ClosedOptions> crossWiredSingleton3,
		CrossWiredSingletonDisposable crossWiredSingleton4)
	{
		ArgumentNullException.ThrowIfNull(crossWiredTransient1);
		ArgumentNullException.ThrowIfNull(crossWiredTransient2);
		ArgumentNullException.ThrowIfNull(crossWiredTransient3);
		ArgumentNullException.ThrowIfNull(crossWiredTransient4);
		ArgumentNullException.ThrowIfNull(crossWiredScoped1);
		ArgumentNullException.ThrowIfNull(crossWiredScoped2);
		ArgumentNullException.ThrowIfNull(crossWiredScoped3);
		ArgumentNullException.ThrowIfNull(crossWiredScoped4);
		ArgumentNullException.ThrowIfNull(crossWiredSingleton1);
		ArgumentNullException.ThrowIfNull(crossWiredSingleton2);
		ArgumentNullException.ThrowIfNull(crossWiredSingleton3);
		ArgumentNullException.ThrowIfNull(crossWiredSingleton4);
	}
}

public class ViewComponentCrossWired : ViewComponent
{
	public ViewComponentCrossWired
	(
		CrossWiredTransient crossWiredTransient1,
		CrossWiredTransientGeneric<OpenOptions> crossWiredTransient2,
		CrossWiredTransientGeneric<ClosedOptions> crossWiredTransient3,
		CrossWiredTransientDisposable crossWiredTransient4,
		CrossWiredScoped crossWiredScoped1,
		CrossWiredScopedGeneric<OpenOptions> crossWiredScoped2,
		CrossWiredScopedGeneric<ClosedOptions> crossWiredScoped3,
		CrossWiredScopedDisposable crossWiredScoped4,
		CrossWiredSingleton crossWiredSingleton1,
		CrossWiredSingletonGeneric<OpenOptions> crossWiredSingleton2,
		CrossWiredSingletonGeneric<ClosedOptions> crossWiredSingleton3,
		CrossWiredSingletonDisposable crossWiredSingleton4)

	{
		ArgumentNullException.ThrowIfNull(crossWiredTransient1);
		ArgumentNullException.ThrowIfNull(crossWiredTransient2);
		ArgumentNullException.ThrowIfNull(crossWiredTransient3);
		ArgumentNullException.ThrowIfNull(crossWiredTransient4);
		ArgumentNullException.ThrowIfNull(crossWiredScoped1);
		ArgumentNullException.ThrowIfNull(crossWiredScoped2);
		ArgumentNullException.ThrowIfNull(crossWiredScoped3);
		ArgumentNullException.ThrowIfNull(crossWiredScoped4);
		ArgumentNullException.ThrowIfNull(crossWiredSingleton1);
		ArgumentNullException.ThrowIfNull(crossWiredSingleton2);
		ArgumentNullException.ThrowIfNull(crossWiredSingleton3);
		ArgumentNullException.ThrowIfNull(crossWiredSingleton4);
	}
}