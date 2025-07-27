namespace Castle.Windsor.Tests.ClassComponents;

public class CustomerChain3 : CustomerChain1
{
    public CustomerChain3(ICustomer customer) : base(customer)
    {
    }
}