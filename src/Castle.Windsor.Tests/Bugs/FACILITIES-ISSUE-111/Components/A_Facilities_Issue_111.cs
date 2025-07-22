using System;
using Castle.Windsor.Core;
using JetBrains.Annotations;

namespace Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111.Components;

[UsedImplicitly]
public class AFacilitiesIssue111 : IAFacilitiesIssue111, IStartable
{
	private IBFacilitiesIssue111[] _ibs;

	public AFacilitiesIssue111(IBFacilitiesIssue111[] ibs)
	{
		_ibs = ibs;
	}

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