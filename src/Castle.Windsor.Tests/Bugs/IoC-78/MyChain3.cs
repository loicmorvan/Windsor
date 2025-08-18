namespace Castle.Windsor.Tests.Bugs.IoC_78;

public class MyChain3 : IChain
{
    // ReSharper disable once MemberCanBeMadeStatic.Global
#pragma warning disable CA1822
    public IChain Chain => null;
#pragma warning restore CA1822
}