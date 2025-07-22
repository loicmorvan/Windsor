using System;
using JetBrains.Annotations;

namespace Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111.Components;

[UsedImplicitly]
public class BFacilitiesIssue111 : IBFacilitiesIssue111
{
	public void Method()
	{
		Console.WriteLine("B: Method");
	}
}