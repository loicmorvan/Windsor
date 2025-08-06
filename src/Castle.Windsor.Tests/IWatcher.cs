using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

public interface IWatcher
{
    [UsedImplicitly] event Action<string> OnSomethingInterestingToWatch;
}