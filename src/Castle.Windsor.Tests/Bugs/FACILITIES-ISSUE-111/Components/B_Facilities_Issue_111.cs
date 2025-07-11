using System;

namespace Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111.Components;

public class BFacilitiesIssue111 : IBFacilitiesIssue111
{
	public void Method()
	{
		Console.WriteLine("B: Method");
	}
}