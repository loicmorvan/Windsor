namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class CompositeViewComponent
{
    public CompositeViewComponent(
        ViewComponentCrossWired crossWiredViewComponent,
        ViewComponentServiceProviderOnly serviceProviderOnlyViewComponent,
        ViewComponentWindsorOnly windsorOnlyViewComponent)
    {
        ArgumentNullException.ThrowIfNull(crossWiredViewComponent);
        ArgumentNullException.ThrowIfNull(serviceProviderOnlyViewComponent);
        ArgumentNullException.ThrowIfNull(windsorOnlyViewComponent);
    }
}