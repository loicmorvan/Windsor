namespace Castle.Windsor.Tests.Bugs.IoC_78;

public class MyChain : IChain
{
    public MyChain()
    {
    }

    // ReSharper disable once UnusedParameter.Local
    public MyChain(IChain chain)
    {
    }
}