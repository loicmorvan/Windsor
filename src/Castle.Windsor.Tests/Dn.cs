using Castle.Windsor.Core;

namespace Castle.Windsor.Tests;

[Transient]
public class Dn : IN
{
    public Dn(IWm vm, ISp sp)
    {
        Cs = new Bs();
    }

    public IS Cs { get; }
}