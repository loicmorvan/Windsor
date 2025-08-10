using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Castle.Windsor.Facilities.AspNetCore.Tests.Fakes;

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