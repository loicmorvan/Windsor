using Castle.Windsor.MicroKernel.Lifestyle;

namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class AnyComponentWithLifestyleManager : AbstractLifestyleManager
{
    public override void Dispose()
    {
    }
}