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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;

namespace Castle.Windsor.Windsor.Configuration.Interpreters.XmlProcessor;

public class DefaultXmlProcessorNodeList : IXmlProcessorNodeList
{
    private readonly IList<XmlNode> _nodes;

    public DefaultXmlProcessorNodeList(XmlNode node)
    {
        _nodes = new List<XmlNode>();
        _nodes.Add(node);
    }

    public DefaultXmlProcessorNodeList(IList<XmlNode> nodes)
    {
        _nodes = nodes;
    }

    public DefaultXmlProcessorNodeList(XmlNodeList nodes)
    {
        _nodes = CloneNodeList(nodes);
    }

    public int Count => _nodes.Count;

    public XmlNode Current => _nodes[CurrentPosition];

    public int CurrentPosition { get; set; } = -1;

    public bool HasCurrent => CurrentPosition < _nodes.Count;

    public bool MoveNext()
    {
        return ++CurrentPosition < _nodes.Count;
    }

    /// <summary>Make a shallow copy of the nodeList.</summary>
    /// <param name="nodeList">The nodeList to be copied.</param>
    /// <returns></returns>
    [PublicAPI]
    protected static IList<XmlNode> CloneNodeList(XmlNodeList nodeList)
    {
        var nodes = new List<XmlNode>(nodeList.Count);
        nodes.AddRange(nodeList.Cast<XmlNode>());

        return nodes;
    }
}