using Castle.Core.Configuration;
using Castle.Windsor.MicroKernel;

namespace Castle.Windsor.Tests.Config;

public class DummyFacility : IFacility
{
    public void Init(IKernel kernel, IConfiguration facilityConfig)
    {
        Assert.NotNull(facilityConfig);
        var childItem = facilityConfig.Children["item"];
        Assert.NotNull(childItem);
        Assert.Equal("value", childItem.Value);
    }

    public void Terminate()
    {
    }
}