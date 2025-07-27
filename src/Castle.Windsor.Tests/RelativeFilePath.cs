namespace Castle.Windsor.Tests.IOC51;

public class RelativeFilePath(IPathProvider basePathProvider, string extensionsPath) : IPathProvider
{
    public string Path { get; } = System.IO.Path.Combine(basePathProvider.Path + "\\", extensionsPath);
}