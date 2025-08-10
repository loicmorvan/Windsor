namespace Castle.Windsor.Tests;

public class BirdWatcher : IWatcher
{
    public event Action<string> OnSomethingInterestingToWatch = delegate { };
}