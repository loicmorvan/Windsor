namespace Castle.Windsor.Tests.Bugs.IoC_78;

public class MyChain2 : IChain
{
    public MyChain2()
    {
    }

    public MyChain2(IChain chain)
    {
    }
}