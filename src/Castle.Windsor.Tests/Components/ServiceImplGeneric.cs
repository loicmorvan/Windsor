namespace Castle.Windsor.Tests.Components;

public class ServiceImplGeneric<T> : IService
{
	public string Name { get; set; }
}