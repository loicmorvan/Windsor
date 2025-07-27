using System;
using Castle.Windsor.Core;

// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global

namespace Castle.Windsor.Tests;

[Transient]
public class AnyClass
{
    public AnyClass(int integer)
    {
    }

    public AnyClass(DateTime datetime)
    {
    }
}