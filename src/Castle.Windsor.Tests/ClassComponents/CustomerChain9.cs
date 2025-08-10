namespace Castle.Windsor.Tests.ClassComponents;

[Serializable]
public class CustomerChain9 : CustomerChain1
{
    public CustomerChain9(ICustomer customer) : base(customer)
    {
    }
}