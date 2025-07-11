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

namespace Castle.Windsor.Configuration.Interpreters.XmlProcessor;

using System.Collections.Generic;
using System.Xml;

public class DefaultXmlProcessorNodeList : IXmlProcessorNodeList
{
	private readonly IList<XmlNode> _nodes;
	private int _index = -1;

	public DefaultXmlProcessorNodeList(XmlNode node)
	{
		_nodes = new List<XmlNode>();
		_nodes.Add(node);
	}

	public DefaultXmlProcessorNodeList(IList<XmlNode> nodes)
	{
		this._nodes = nodes;
	}

	public DefaultXmlProcessorNodeList(XmlNodeList nodes)
	{
		this._nodes = CloneNodeList(nodes);
	}

	public int Count
	{
		get { return _nodes.Count; }
	}

	public XmlNode Current
	{
		get { return _nodes[_index]; }
	}

	public int CurrentPosition
	{
		get { return _index; }
		set { _index = value; }
	}

	public bool HasCurrent
	{
		get { return _index < _nodes.Count; }
	}

	public bool MoveNext()
	{
		return ++_index < _nodes.Count;
	}

	/// <summary>
	///   Make a shallow copy of the nodeList.
	/// </summary>
	/// <param name = "nodeList">The nodeList to be copied.</param>
	/// <returns></returns>
	protected IList<XmlNode> CloneNodeList(XmlNodeList nodeList)
	{
		IList<XmlNode> nodes = new List<XmlNode>(nodeList.Count);

		foreach (XmlNode node in nodeList)
		{
			nodes.Add(node);
		}

		return nodes;
	}
}