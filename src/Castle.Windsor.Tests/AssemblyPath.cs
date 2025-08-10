using System.Reflection;
using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

[UsedImplicitly]
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