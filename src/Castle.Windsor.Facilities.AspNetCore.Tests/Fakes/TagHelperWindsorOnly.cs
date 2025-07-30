using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Castle.Windsor.Facilities.AspNetCore.Tests.Fakes;

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