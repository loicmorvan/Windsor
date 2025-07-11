namespace Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111.Components;

using System;

using Castle.Core;

public class AFacilitiesIssue111(IBFacilitiesIssue111[] ibs) : IAFacilitiesIssue111, IStartable
{
	IBFacilitiesIssue111[] _ibs = ibs;

	public void Method()
	{
		Console.WriteLine("A: Method");
	}

	public void Start()
	{
		Console.WriteLine("Started A");
	}

	public void Stop()
	{
		Console.WriteLine("Stopped A");
	}
}