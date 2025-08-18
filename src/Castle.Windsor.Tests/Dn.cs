using Castle.Windsor.Core;
using JetBrains.Annotations;

// ReSharper disable UnusedParameter.Local

namespace Castle.Windsor.Tests;

[Transient]
[UsedImplicitly]
public class Dn : IN
{
    public Dn(IWm vm, ISp sp)
    {
    }
}