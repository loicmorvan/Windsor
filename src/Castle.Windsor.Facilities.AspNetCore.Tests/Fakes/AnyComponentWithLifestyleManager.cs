using Castle.Windsor.MicroKernel.Lifestyle;

namespace Castle.Windsor.Facilities.AspNetCore.Tests.Fakes;

public class AnyComponentWithLifestyleManager : AbstractLifestyleManager
{
    public override void Dispose()
    {
    }
}