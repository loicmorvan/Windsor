using Castle.Windsor.Tests.Facilities.TypedFactory;

namespace Castle.Windsor.Tests.Bugs.IoC_169;

public class Blackboard : AbstractBlackboard
{
    // ReSharper disable once UnusedParameter.Local
    public Blackboard(IChalk chalk, DataRepository dataRepository) : base(dataRepository)
    {
    }
}