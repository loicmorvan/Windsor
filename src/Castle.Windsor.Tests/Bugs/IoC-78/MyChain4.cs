namespace Castle.Windsor.Tests.Bugs.IoC_78;

public class MyChain4 : IChain
{
    // ReSharper disable once UnusedParameter.Local
    public MyChain4(IChain chain)
    {
    }
}