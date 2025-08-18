namespace Castle.Windsor.Tests.ClassComponents;

[Serializable]
public class CustomerChain8(ICustomer customer) : CustomerChain1(customer);