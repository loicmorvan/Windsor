using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

[UsedImplicitly]
#pragma warning disable CS9113 // Parameter is unread.
public class MyObject(IDictionary<int, IList<string>> stuff) : IMyObject;
#pragma warning restore CS9113 // Parameter is unread.
