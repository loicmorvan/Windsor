using System;
using Castle.Core;

namespace Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111.Components;

public class AFacilitiesIssue111(IBFacilitiesIssue111[] ibs) : IAFacilitiesIssue111, IStartable
{
	private IBFacilitiesIssue111[] _ibs = ibs;

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