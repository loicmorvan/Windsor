using Castle.Windsor.Core;
using Castle.Windsor.Tests.Facilities.TypedFactory;

namespace Castle.Windsor.Tests.Facilities.Startable.Components;

public class StartableChainDependency : IStartable
{
    private readonly LifecycleCounter _lifecycleCounter;

    public StartableChainDependency(StartableChainGeneric<string> item, LifecycleCounter lifecycleCounter)
    {
        _lifecycleCounter = lifecycleCounter;
        _lifecycleCounter.Increment("Create");
    }

    public void Start()
    {
        _lifecycleCounter.Increment("Start");
    }

    public void Stop()
    {
        _lifecycleCounter.Increment("Stop");
    }
}