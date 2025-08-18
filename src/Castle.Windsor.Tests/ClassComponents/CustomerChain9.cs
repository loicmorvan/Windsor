namespace Castle.Windsor.Tests.ClassComponents;

[Serializable]
public class CustomerChain9(ICustomer customer) : CustomerChain1(customer);