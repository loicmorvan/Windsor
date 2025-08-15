namespace Castle.Windsor.Tests.ClassComponents;

public class CustomerChain3(ICustomer customer) : CustomerChain1(customer);