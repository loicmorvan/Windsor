using System;
using Microsoft.AspNetCore.Mvc;

namespace Castle.Facilities.AspNetCore.Tests.Fakes;

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