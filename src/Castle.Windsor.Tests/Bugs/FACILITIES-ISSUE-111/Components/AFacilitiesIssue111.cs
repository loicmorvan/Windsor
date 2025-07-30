using Castle.Windsor.Core;
using JetBrains.Annotations;

namespace Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111.Components;

[UsedImplicitly]
public class AFacilitiesIssue111 : IAFacilitiesIssue111, IStartable
{
    public AFacilitiesIssue111(IBFacilitiesIssue111[] ibs)
    {
    }

    public void Method()
    {
        Console.WriteLine(@"A: Method");
    }

    public void Start()
    {
        Console.WriteLine(@"Started A");
    }

    public void Stop()
    {
        Console.WriteLine(@"Stopped A");
    }
}