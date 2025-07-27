namespace Castle.Windsor.Tests.Bugs.IoC_78;

public class MyChain4 : IChain
{
    public MyChain4(IChain chain)
    {
    }
}