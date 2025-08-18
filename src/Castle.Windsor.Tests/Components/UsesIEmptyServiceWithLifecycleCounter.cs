using Castle.Windsor.Tests.Facilities.TypedFactory;

namespace Castle.Windsor.Tests.Components;

public class UsesIEmptyServiceWithLifecycleCounter
{
    // ReSharper disable once UnusedParameter.Local
    public UsesIEmptyServiceWithLifecycleCounter(IEmptyService emptyService, DataRepository counter)
    {
        counter.RegisterCallerMemberName();
    }
}