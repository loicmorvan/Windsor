namespace Castle.Windsor.Tests.ClassComponents;

public class CustomerChainValidator<T> : IValidator<T>
    where T : CustomerChain1
{
    /// <summary></summary>
    /// <param name="customerChain"></param>
    /// <returns></returns>
    public bool IsValid(T customerChain)
    {
        return true;
    }
}