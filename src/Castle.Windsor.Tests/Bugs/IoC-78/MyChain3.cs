namespace Castle.Windsor.Tests.Bugs.IoC_78;

public class MyChain3 : IChain
{
    public IChain Chain { get; set; }
}