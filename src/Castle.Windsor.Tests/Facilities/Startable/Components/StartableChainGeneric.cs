using Castle.Windsor.Tests.Facilities.TypedFactory;

namespace Castle.Windsor.Tests.Facilities.Startable.Components;

public class StartableChainGeneric<T>
{
    public StartableChainGeneric(LifecycleCounter lifecycleCounter)
    {
        lifecycleCounter.Increment("Create");
    }
}