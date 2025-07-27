// Copyright 2004-2022 Castle Project - http://www.castleproject.org/
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using Castle.Core.Resource;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel.SubSystems.Resource;
using Castle.Windsor.Windsor.Configuration.Interpreters.XmlProcessor.ElementProcessors;

namespace Castle.Windsor.Windsor.Configuration.Interpreters.XmlProcessor;

public class DefaultXmlProcessorEngine : IXmlProcessorEngine
{
	private readonly IXmlNodeProcessor _defaultElementProcessor;
	private readonly Regex _flagPattern = new(@"^(\w|_)+$");
	private readonly IDictionary<string, bool> _flags = new Dictionary<string, bool>();

	private readonly IDictionary<XmlNodeType, IDictionary<string, IXmlNodeProcessor>> _nodeProcessors =
		new Dictionary<XmlNodeType, IDictionary<string, IXmlNodeProcessor>>();

	private readonly IDictionary<string, XmlElement> _properties = new Dictionary<string, XmlElement>();
	private readonly Stack<IResource> _resourceStack = new();

	private readonly IResourceSubSystem _resourceSubSystem;

	/// <summary>Initializes a new instance of the <see cref="DefaultXmlProcessorEngine" /> class.</summary>
	/// <param name="environmentName">Name of the environment.</param>
	public DefaultXmlProcessorEngine(string environmentName) : this(environmentName, new DefaultResourceSubSystem())
	{
	}

	/// <summary>Initializes a new instance of the <see cref="DefaultXmlProcessorEngine" /> class.</summary>
	/// <param name="environmentName">Name of the environment.</param>
	/// <param name="resourceSubSystem">The resource sub system.</param>
	public DefaultXmlProcessorEngine(string environmentName, IResourceSubSystem resourceSubSystem)
	{
		AddEnvNameAsFlag(environmentName);
		_resourceSubSystem = resourceSubSystem;
		_defaultElementProcessor = new DefaultElementProcessor();
	}

	public void AddFlag(string flag)
	{
		_flags[GetCanonicalFlagName(flag)] = true;
	}

	public void AddNodeProcessor(Type type)
	{
		if (type.Is<IXmlNodeProcessor>())
		{
			var processor = type.CreateInstance<IXmlNodeProcessor>();
			foreach (var nodeType in processor.AcceptNodeTypes) RegisterProcessor(nodeType, processor);
		}
		else
		{
			throw new XmlProcessorException("{0} does not implement {1} interface", type.FullName,
				typeof(IXmlNodeProcessor).FullName);
		}
	}

	public void AddProperty(XmlElement content)
	{
		_properties[content.Name] = content;
	}

	/// <summary>Processes the element.</summary>
	/// <param name="nodeList">The element.</param>
	/// <returns></returns>
	public void DispatchProcessAll(IXmlProcessorNodeList nodeList)
	{
		while (nodeList.MoveNext()) DispatchProcessCurrent(nodeList);
	}

	/// <summary>Processes the element.</summary>
	/// <param name="nodeList">The element.</param>
	/// <returns></returns>
	public void DispatchProcessCurrent(IXmlProcessorNodeList nodeList)
	{
		var processor = GetProcessor(nodeList.Current);

		processor?.Process(nodeList, this);
	}

	public XmlElement GetProperty(string key)
	{
		if (!_properties.TryGetValue(key, out var property))
		{
			return null;
		}

		return property.CloneNode(true) as XmlElement;
	}

	public IResource GetResource(string uri)
	{
		IResource resource;
		if (_resourceStack.Count > 0)
			resource = _resourceStack.Peek();
		else
			resource = null;

		if (uri.IndexOf(Uri.SchemeDelimiter, StringComparison.Ordinal) != -1)
		{
			if (resource == null) return _resourceSubSystem.CreateResource(uri);

			return _resourceSubSystem.CreateResource(uri, resource.FileBasePath);
		}

		if (_resourceStack.Count <= 0)
			throw new XmlProcessorException("Cannot get relative resource '" + uri + "', resource stack is empty");

		// NOTE: what if resource is null at this point?
		Debug.Assert(resource != null, nameof(resource) + " != null");
		return resource.CreateRelative(uri);
	}

	public bool HasFlag(string flag)
	{
		return _flags.ContainsKey(GetCanonicalFlagName(flag));
	}

	public bool HasProperty(string name)
	{
		return _properties.ContainsKey(name);
	}

	public bool HasSpecialProcessor(XmlNode node)
	{
		return GetProcessor(node) != _defaultElementProcessor;
	}

	public void PopResource()
	{
		_resourceStack.Pop();
	}

	public void PushResource(IResource resource)
	{
		_resourceStack.Push(resource);
	}

	public void RemoveFlag(string flag)
	{
		_flags.Remove(GetCanonicalFlagName(flag));
	}

	private void AddEnvNameAsFlag(string environmentName)
	{
		if (environmentName != null) AddFlag(environmentName);
	}

	private string GetCanonicalFlagName(string flag)
	{
		flag = flag.Trim().ToLower();

		if (!_flagPattern.IsMatch(flag)) throw new XmlProcessorException("Invalid flag name '{0}'", flag);

		return flag;
	}

	private IXmlNodeProcessor GetProcessor(XmlNode node)
	{
		if (!_nodeProcessors.TryGetValue(node.NodeType, out var processors))
		{
			return null;
		}

		// sometimes nodes with the same name will not accept a processor
		if (!processors.TryGetValue(node.Name, out var processor) || !processor.Accept(node))
			if (node.NodeType == XmlNodeType.Element)
				processor = _defaultElementProcessor;

		return processor;
	}

	private void RegisterProcessor(XmlNodeType type, IXmlNodeProcessor processor)
	{
		if (!_nodeProcessors.TryGetValue(type, out var typeProcessors))
		{
			typeProcessors = new Dictionary<string, IXmlNodeProcessor>();
			_nodeProcessors[type] = typeProcessors;
		}

		if (typeProcessors.ContainsKey(processor.Name))
			throw new XmlProcessorException("There is already a processor register for {0} with name {1} ", type,
				processor.Name);

		typeProcessors.Add(processor.Name, processor);
	}
}