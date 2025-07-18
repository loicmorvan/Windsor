// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.MicroKernel.Registration;

using System;

using Castle.Core.Configuration;

#region Node

/// <summary>Represents a configuration child.</summary>
public abstract class Node
{
	protected Node(string name)
	{
		Name = name;
	}

	protected string Name { get; }

	/// <summary>Applies the configuration node.</summary>
	/// <param name = "configuration">The configuration.</param>
	public abstract void ApplyTo(IConfiguration configuration);
}

#endregion

#region Attribute

/// <summary>Represents a configuration attribute.</summary>
public class Attrib : Node
{
	private readonly string value;

	internal Attrib(string name, string value)
		: base(name)
	{
		this.value = value;
	}

	/// <summary>Applies the configuration node.</summary>
	/// <param name = "configuration">The configuration.</param>
	public override void ApplyTo(IConfiguration configuration)
	{
		configuration.Attributes.Add(Name, value);
	}

	/// <summary>Create a <see cref = "NamedAttribute" /> with name.</summary>
	/// <param name = "name">The attribute name.</param>
	/// <returns>The new <see cref = "NamedAttribute" /></returns>
	public static NamedAttribute ForName(string name)
	{
		return new NamedAttribute(name);
	}
}

#endregion

#region NamedChild

/// <summary>Represents a named attribute.</summary>
public class NamedAttribute
{
	private readonly string name;

	internal NamedAttribute(string name)
	{
		this.name = name;
	}

	/// <summary>Builds the <see cref = "Attribute" /> with name/value.</summary>
	/// <param name = "value">The attribute value.</param>
	/// <returns>The new <see cref = "SimpleChild" /></returns>
	public Attrib Eq(string value)
	{
		return new Attrib(name, value);
	}

	/// <summary>Builds the <see cref = "Attribute" /> with name/value.</summary>
	/// <param name = "value">The attribute value.</param>
	/// <returns>The new <see cref = "SimpleChild" /></returns>
	public Attrib Eq(object value)
	{
		var valueStr = value != null ? value.ToString() : string.Empty;
		return new Attrib(name, valueStr);
	}
}

#endregion

#region Child

/// <summary>Represents a configuration child.</summary>
public abstract class Child
{
	/// <summary>Create a <see cref = "NamedChild" /> with name.</summary>
	/// <param name = "name">The child name.</param>
	/// <returns>The new <see cref = "NamedChild" /></returns>
	public static NamedChild ForName(string name)
	{
		return new NamedChild(name);
	}
}

#endregion

#region NamedChild

/// <summary>Represents a named child.</summary>
public class NamedChild : Node
{
	internal NamedChild(string name)
		: base(name)
	{
	}

	/// <summary>Applies the configuration node.</summary>
	/// <param name = "configuration">The configuration.</param>
	public override void ApplyTo(IConfiguration configuration)
	{
		var node = new MutableConfiguration(Name);
		configuration.Children.Add(node);
	}

	/// <summary>Builds the <see cref = "SimpleChild" /> with name/value.</summary>
	/// <param name = "value">The child value.</param>
	/// <returns>The new <see cref = "SimpleChild" /></returns>
	public SimpleChild Eq(string value)
	{
		return new SimpleChild(Name, value);
	}

	/// <summary>Builds the <see cref = "SimpleChild" /> with name/value.</summary>
	/// <param name = "value">The child value.</param>
	/// <returns>The new <see cref = "SimpleChild" /></returns>
	public SimpleChild Eq(object value)
	{
		var valueStr = value != null ? value.ToString() : string.Empty;
		return new SimpleChild(Name, valueStr);
	}

	/// <summary>Builds the <see cref = "ComplexChild" /> with name/config.</summary>
	/// <param name = "configNode">The child configuration.</param>
	/// <returns>The new <see cref = "ComplexChild" /></returns>
	public ComplexChild Eq(IConfiguration configNode)
	{
		return new ComplexChild(Name, configNode);
	}

	/// <summary>Builds the <see cref = "Child" /> with name/config.</summary>
	/// <param name = "childNodes">The child nodes.</param>
	/// <returns>The new <see cref = "CompoundChild" /></returns>
	public CompoundChild Eq(params Node[] childNodes)
	{
		return new CompoundChild(Name, childNodes);
	}
}

#endregion

#region SimpleChild

/// <summary>Represents a simple child node.</summary>
public class SimpleChild : Node
{
	private readonly string value;

	internal SimpleChild(string name, string value)
		: base(name)
	{
		this.value = value;
	}

	/// <summary>Applies the configuration node.</summary>
	/// <param name = "configuration">The configuration.</param>
	public override void ApplyTo(IConfiguration configuration)
	{
		var node = new MutableConfiguration(Name, value);
		configuration.Children.Add(node);
	}
}

#endregion

#region ComplexChild

/// <summary>Represents a complex child node.</summary>
public class ComplexChild : Node
{
	private readonly IConfiguration configNode;

	internal ComplexChild(string name, IConfiguration configNode)
		: base(name)
	{
		this.configNode = configNode;
	}

	/// <summary>Applies the configuration node.</summary>
	/// <param name = "configuration">The configuration.</param>
	public override void ApplyTo(IConfiguration configuration)
	{
		var node = new MutableConfiguration(Name);
		node.Children.Add(configNode);
		configuration.Children.Add(node);
	}
}

#endregion

#region CompoundChild

/// <summary>Represents a compound child node.</summary>
public class CompoundChild : Node
{
	private readonly Node[] childNodes;

	internal CompoundChild(string name, Node[] childNodes)
		: base(name)
	{
		this.childNodes = childNodes;
	}

	/// <summary>Applies the configuration node.</summary>
	/// <param name = "configuration">The configuration.</param>
	public override void ApplyTo(IConfiguration configuration)
	{
		var node = new MutableConfiguration(Name);
		foreach (var childNode in childNodes) childNode.ApplyTo(node);
		configuration.Children.Add(node);
	}
}

#endregion