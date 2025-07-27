namespace Castle.Windsor.Tests.Bugs.IoC_78;

public class MyChain : IChain
{
    public MyChain()
    {
    }

    public MyChain(IChain chain)
    {
    }
}