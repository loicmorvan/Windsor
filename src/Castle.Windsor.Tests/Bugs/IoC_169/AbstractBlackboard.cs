using Castle.Windsor.Core;
using Castle.Windsor.Tests.Facilities.TypedFactory;

namespace Castle.Windsor.Tests.Bugs.IoC_169;

public abstract class AbstractBlackboard(DataRepository dataRepository) : IBlackboard, IStartable
{
    public void Start()
    {
        dataRepository.RegisterCallerMemberName();
    }

    public void Stop()
    {
    }
}