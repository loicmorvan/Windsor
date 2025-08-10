using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

[UsedImplicitly]
#pragma warning disable CS9113 // Parameter is unread.
public class MessageChannel(IDevice root);
#pragma warning restore CS9113 // Parameter is unread.
