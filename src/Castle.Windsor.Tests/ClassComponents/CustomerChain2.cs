namespace Castle.Windsor.Tests.ClassComponents;

public class CustomerChain2(ICustomer customer) : CustomerChain1(customer);