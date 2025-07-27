using Castle.Windsor.Core;

namespace Castle.Windsor.Tests;

[Transient]
public class AnyClassWithReference
{
    public AnyClassWithReference(Tester1 test1)
    {
    }

    public AnyClassWithReference(Tester2 test2)
    {
    }
}