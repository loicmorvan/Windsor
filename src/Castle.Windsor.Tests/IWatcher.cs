namespace Castle.Windsor.Tests;

public interface IWatcher
{
    event Action<string> OnSomethingInterestingToWatch;
}