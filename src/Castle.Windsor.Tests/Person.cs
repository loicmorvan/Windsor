namespace Castle.Windsor.Tests;

public class Person(IWatcher watcher)
{
    public readonly IWatcher Watcher = watcher;
}