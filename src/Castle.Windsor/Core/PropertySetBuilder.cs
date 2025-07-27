using System.Reflection;

namespace Castle.Windsor.Core;

public delegate PropertySet PropertySetBuilder(PropertyInfo property, bool isOptional);