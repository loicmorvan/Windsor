namespace Castle.Windsor.Tests.ClassComponents;

public class CustomerChainValidator<T> : IValidator<T>
    where T : CustomerChain1;