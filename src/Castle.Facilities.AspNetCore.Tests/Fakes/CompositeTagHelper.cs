using System;

namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class CompositeTagHelper
{
    public CompositeTagHelper(
        TagHelperCrossWired crossWiredTagHelper,
        TagHelperServiceProviderOnly serviceProviderOnlyTagHelper,
        TagHelperWindsorOnly windsorOnlyTagHelper)
    {
        ArgumentNullException.ThrowIfNull(crossWiredTagHelper);
        ArgumentNullException.ThrowIfNull(serviceProviderOnlyTagHelper);
        ArgumentNullException.ThrowIfNull(windsorOnlyTagHelper);
    }
}