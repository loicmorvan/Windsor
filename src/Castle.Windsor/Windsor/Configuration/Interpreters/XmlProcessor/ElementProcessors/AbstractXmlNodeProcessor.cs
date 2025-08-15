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

using System.Diagnostics;
using System.Xml;
using JetBrains.Annotations;

namespace Castle.Windsor.Windsor.Configuration.Interpreters.XmlProcessor.ElementProcessors;

public abstract class AbstractXmlNodeProcessor : IXmlNodeProcessor
{
    private static readonly XmlNodeType[] AcceptNodes = [XmlNodeType.Element];

    public abstract string Name { get; }

    public virtual XmlNodeType[] AcceptNodeTypes => AcceptNodes;

    public abstract void Process(IXmlProcessorNodeList nodeList, IXmlProcessorEngine engine);

    /// <summary>
    ///     Accepts the specified node. Check if node has the same name as the processor and the node.NodeType is in the
    ///     AcceptNodeTypes List
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns></returns>
    public virtual bool Accept(XmlNode node)
    {
        return node.Name == Name && Array.IndexOf(AcceptNodeTypes, node.NodeType) != -1;
    }

    protected static void AppendChild(XmlNode element, XmlNodeList nodes)
    {
        var childNodes = new DefaultXmlProcessorNodeList(nodes);

        while (childNodes.MoveNext())
        {
            AppendChild(element, childNodes.Current);
        }
    }

    protected static void AppendChild(XmlNode element, string text)
    {
        AppendChild(element, CreateText(element, text));
    }

    protected static void AppendChild(XmlNode element, XmlNode child)
    {
        element.AppendChild(ImportNode(element, child));
    }

    protected static XmlDocumentFragment CreateFragment(XmlNode parentNode)
    {
        Debug.Assert(parentNode.OwnerDocument != null);
        return parentNode.OwnerDocument.CreateDocumentFragment();
    }

    [PublicAPI]
    protected static XmlText CreateText(XmlNode node, string content)
    {
        Debug.Assert(node.OwnerDocument != null);
        return node.OwnerDocument.CreateTextNode(content);
    }

    /// <summary>
    ///     Convert and return child parameter into an XmlElement An exception will be throw in case the child node cannot
    ///     be converted
    /// </summary>
    /// <param name="element">Parent node</param>
    /// <param name="child">Node to be converted</param>
    /// <returns>child node as XmlElement</returns>
    protected static XmlElement GetNodeAsElement(XmlElement element, XmlNode child)
    {
        return child as XmlElement ??
               throw new XmlProcessorException("{0} expects XmlElement found {1}", element.Name, child.NodeType);
    }

    protected static string GetRequiredAttribute(XmlElement element, string attribute)
    {
        var attValue = element.GetAttribute(attribute).Trim();

        return attValue == string.Empty
            ? throw new XmlProcessorException("'{0}' requires a non empty '{1}' attribute", element.Name, attribute)
            : attValue;
    }

    [PublicAPI]
    protected virtual bool IgnoreNode(XmlNode node)
    {
        return node.NodeType is XmlNodeType.Comment or XmlNodeType.Entity or XmlNodeType.EntityReference;
    }

    protected static XmlNode ImportNode(XmlNode targetElement, XmlNode node)
    {
        Debug.Assert(targetElement.OwnerDocument != null);
        return targetElement.OwnerDocument == node.OwnerDocument
            ? node
            : targetElement.OwnerDocument.ImportNode(node, true);
    }

    [PublicAPI]
    protected static bool IsTextNode(XmlNode node)
    {
        return node.NodeType is XmlNodeType.Text or XmlNodeType.CDATA;
    }

    [PublicAPI]
    protected static void MoveChildNodes(XmlDocumentFragment fragment, XmlElement element)
    {
        while (element.ChildNodes.Count > 0)
        {
            fragment.AppendChild(element.ChildNodes[0] ?? throw new InvalidOperationException());
        }
    }

    protected static void RemoveItSelf(XmlNode node)
    {
        Debug.Assert(node.ParentNode != null);
        node.ParentNode.RemoveChild(node);
    }

    protected static void ReplaceItself(XmlNode newNode, XmlNode oldNode)
    {
        ReplaceNode(oldNode.ParentNode, newNode, oldNode);
    }

    protected static void ReplaceNode(XmlNode element, XmlNode newNode, XmlNode oldNode)
    {
        if (newNode == oldNode)
        {
            return;
        }

        var importedNode = ImportNode(element, newNode);

        element.ReplaceChild(importedNode, oldNode);
    }
}