namespace Castle.Windsor.Tests.Configuration2;

public class MyConfiguration : IMyConfiguration
{
    public MyConfiguration(int port)
    {
        Port = port;
    }

    public int Port { get; }
}