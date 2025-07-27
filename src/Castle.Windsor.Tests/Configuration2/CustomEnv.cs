using Castle.Windsor.Windsor;

namespace Castle.Windsor.Tests.Configuration2;

internal class CustomEnv(bool isDevelopment) : IEnvironmentInfo
{
    public string GetEnvironmentName()
    {
        return isDevelopment ? "devel" : "test";
    }
}