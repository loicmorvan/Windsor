using Castle.Windsor.Core;
using JetBrains.Annotations;

namespace Castle.Windsor.Tests.RuntimeParameters;

[Transient]
[UsedImplicitly]
public class CompB
{
    public CompB(CompA ca, CompC cc, string myArgument)
    {
        Compc = cc;
        MyArgument = myArgument;
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public CompC Compc { get; set; }

    public string MyArgument { get; }
}