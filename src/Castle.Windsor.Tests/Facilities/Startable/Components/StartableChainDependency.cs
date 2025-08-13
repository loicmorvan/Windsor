using Castle.Windsor.Core;
using Castle.Windsor.Tests.Facilities.TypedFactory;

namespace Castle.Windsor.Tests.Facilities.Startable.Components;

public class StartableChainDependency : IStartable
{
    private readonly DataRepository _dataRepository;

    // ReSharper disable once UnusedParameter.Local
    public StartableChainDependency(StartableChainGeneric<string> item, DataRepository dataRepository)
    {
        _dataRepository = dataRepository;
        _dataRepository.RegisterCallerMemberName();
    }

    public void Start()
    {
        _dataRepository.RegisterCallerMemberName();
    }

    public void Stop()
    {
        _dataRepository.RegisterCallerMemberName();
    }
}