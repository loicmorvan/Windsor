namespace Castle.Windsor.Tests.ClassComponents;

[Serializable]
public class CustomerChain8 : CustomerChain1
{
    public CustomerChain8(ICustomer customer) : base(customer)
    {
    }
}