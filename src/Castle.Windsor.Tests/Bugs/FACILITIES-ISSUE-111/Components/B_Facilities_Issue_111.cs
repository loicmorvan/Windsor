namespace Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111.Components;

using System;

public class B_Facilities_Issue_111 : IB_Facilities_Issue_111
{
	public void Method()
	{
		Console.WriteLine("B: Method");
	}
}