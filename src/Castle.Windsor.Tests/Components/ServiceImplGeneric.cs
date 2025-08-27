namespace Castle.Windsor.Tests.Components;

// ReSharper disable once UnusedTypeParameter
public class ServiceImplGeneric<T> : IService
{
    public string? Name { get; set; }
}