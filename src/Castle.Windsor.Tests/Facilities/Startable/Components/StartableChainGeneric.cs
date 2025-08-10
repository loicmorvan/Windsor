using Castle.Windsor.Tests.Facilities.TypedFactory;

namespace Castle.Windsor.Tests.Facilities.Startable.Components;

// ReSharper disable once UnusedTypeParameter
public class StartableChainGeneric<T>
{
    public StartableChainGeneric(LifecycleCounter lifecycleCounter)
    {
        lifecycleCounter.Increment();
    }
}