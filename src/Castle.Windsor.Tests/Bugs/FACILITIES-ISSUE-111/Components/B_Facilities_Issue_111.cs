namespace Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111.Components;

using System;

public class BFacilitiesIssue111 : IBFacilitiesIssue111
{
	public void Method()
	{
		Console.WriteLine("B: Method");
	}
}