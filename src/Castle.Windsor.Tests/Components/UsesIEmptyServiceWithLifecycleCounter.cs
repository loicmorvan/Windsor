using Castle.Windsor.Tests.Facilities.TypedFactory;

namespace Castle.Windsor.Tests.Components;

public class UsesIEmptyServiceWithLifecycleCounter
{
    public UsesIEmptyServiceWithLifecycleCounter(IEmptyService emptyService, DataRepository counter)
    {
        counter.RegisterCallerMemberName();

        EmptyService = emptyService;
    }

    public IEmptyService EmptyService { get; }
}