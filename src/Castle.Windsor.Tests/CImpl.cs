namespace Castle.Windsor.Tests;

public class CImpl : IC
{
    public R R
    {
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    public IN N { get; set; } = null;
}