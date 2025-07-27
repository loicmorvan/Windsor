using System;
using Castle.Windsor.Core;

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