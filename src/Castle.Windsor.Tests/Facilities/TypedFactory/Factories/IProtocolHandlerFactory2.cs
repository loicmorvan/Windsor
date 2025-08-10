using Castle.Windsor.Tests.Facilities.TypedFactory.Components;

namespace Castle.Windsor.Tests.Facilities.TypedFactory.Factories;

public interface IProtocolHandlerFactory2
{
    IProtocolHandler Create(string key);

    void Release(IProtocolHandler handler);
}