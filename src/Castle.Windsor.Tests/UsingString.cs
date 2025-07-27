namespace Castle.Windsor.Tests;

public class UsingString
{
    public UsingString(string parameter)
    {
        Parameter = parameter;
    }

    public string Parameter { get; }
}