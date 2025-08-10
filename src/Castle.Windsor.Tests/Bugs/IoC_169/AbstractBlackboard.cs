using Castle.Windsor.Core;

namespace Castle.Windsor.Tests.Bugs.IoC_169;

public abstract class AbstractBlackboard : IBlackboard, IStartable
{
    public static bool Started;

    public void Start()
    {
        Started = true;
    }

    public void Stop()
    {
    }

    public static void PrepareForTest()
    {
        Started = false;
    }
}