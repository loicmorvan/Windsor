using System;
using System.Reflection;

namespace Castle.Windsor.Tests.IOC51;

public class AssemblyPath : IPathProvider
{
    public string Path
    {
        get
        {
            var uriPath = new Uri(typeof(AssemblyPath).GetTypeInfo().Assembly.Location);
            return uriPath.LocalPath;
        }
    }
}