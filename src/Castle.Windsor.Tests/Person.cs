namespace Castle.Windsor.Tests;

public class Person
{
    public readonly IWatcher Watcher;

    public Person(IWatcher watcher)
    {
        Watcher = watcher;
    }
}