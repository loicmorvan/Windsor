using System;
using Castle.Core;

// ReSharper disable LocalizableElement

namespace Castle.Windsor.Tests.Bugs.FACILITIES_ISSUE_111.Components;

// ReSharper disable once UnusedType.Global
#pragma warning disable CS9113 // Parameter is unread.
public class AFacilitiesIssue111(IBFacilitiesIssue111[] ibs) : IAFacilitiesIssue111, IStartable
#pragma warning restore CS9113 // Parameter is unread.
{
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