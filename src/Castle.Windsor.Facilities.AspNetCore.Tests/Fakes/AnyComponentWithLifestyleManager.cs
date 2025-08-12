using Castle.Windsor.MicroKernel.Lifestyle;

namespace Castle.Windsor.Facilities.AspNetCore.Tests.Fakes;

public sealed class AnyComponentWithLifestyleManager : AbstractLifestyleManager
{
    public override void Dispose()
    {
    }
}