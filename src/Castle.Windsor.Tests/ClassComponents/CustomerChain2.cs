namespace Castle.Windsor.Tests.ClassComponents;

public class CustomerChain2 : CustomerChain1
{
    public CustomerChain2(ICustomer customer) : base(customer)
    {
    }
}