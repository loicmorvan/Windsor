using Castle.Windsor.Core;

namespace Castle.Windsor.Tests;

[Transient]
public class Bs : IS
{
    public ISp Sp { get; set; }
}