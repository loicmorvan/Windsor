namespace Castle.Windsor.Tests;

[DefaultImplementation(typeof(Implementation))]
public interface IHasDefaultImplementation
{
    void Foo();
}