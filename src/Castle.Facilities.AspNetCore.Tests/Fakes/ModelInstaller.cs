using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Windsor;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Castle.Facilities.AspNetCore.Tests.Fakes;

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
        container.Register(Component.For<CrossWiredTransientGeneric<ClosedOptions>>().CrossWired()
            .LifestyleTransient());
        container.Register(Component.For<CrossWiredTransientDisposable>().CrossWired().LifestyleTransient());

        container.Register(Component.For<CrossWiredScoped>().CrossWired().LifestyleScoped());
        container.Register(Component.For<CrossWiredScopedGeneric<OpenOptions>>().CrossWired().LifestyleScoped());
        container.Register(Component.For<CrossWiredScopedGeneric<ClosedOptions>>().CrossWired().LifestyleScoped());
        container.Register(Component.For<CrossWiredScopedDisposable>().CrossWired().LifestyleScoped());

        container.Register(Component.For<CrossWiredSingleton>().CrossWired().LifestyleSingleton());
        container.Register(Component.For<CrossWiredSingletonGeneric<OpenOptions>>().CrossWired().LifestyleSingleton());
        container.Register(Component.For<CrossWiredSingletonGeneric<ClosedOptions>>().CrossWired()
            .LifestyleSingleton());
        container.Register(Component.For<CrossWiredSingletonDisposable>().CrossWired().LifestyleSingleton());

        container.Register(Component.For<OpenOptions>().CrossWired().LifestyleSingleton());
        container.Register(Component.For<ControllerCrossWired>().CrossWired().LifestyleScoped());
        container.Register(Component.For<TagHelperCrossWired>().CrossWired().LifestyleScoped());
        container.Register(Component.For<ViewComponentCrossWired>().CrossWired().LifestyleScoped());
    }
}