using Castle.Windsor.Core;

namespace Castle.Windsor.Tests.Bugs.Ioc113;

public sealed class StartableDisposableAndInitializableComponent : IInitializable, IDisposable, IStartable
{
    public readonly List<SdiComponentMethods> CalledMethods = [];

    public void Dispose()
    {
        CalledMethods.Add(SdiComponentMethods.Dispose);
    }

    public void Initialize()
    {
        CalledMethods.Add(SdiComponentMethods.Initialize);
    }

    public void Start()
    {
        CalledMethods.Add(SdiComponentMethods.Start);
    }

    public void Stop()
    {
        CalledMethods.Add(SdiComponentMethods.Stop);
    }

    public void DoSomething()
    {
        CalledMethods.Add(SdiComponentMethods.DoSomething);
    }
}