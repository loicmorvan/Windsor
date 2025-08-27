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
using System.Text.RegularExpressions;
using System.Xml;

namespace Castle.Windsor.Windsor.Configuration.Interpreters.XmlProcessor.ElementProcessors;

public partial class DefaultTextNodeProcessor : AbstractXmlNodeProcessor
{
    private static readonly XmlNodeType[] AcceptNodes = [XmlNodeType.CDATA, XmlNodeType.Text];

    public override XmlNodeType[] AcceptNodeTypes => AcceptNodes;

    public override string Name => "#text";

    public override void Process(IXmlProcessorNodeList nodeList, IXmlProcessorEngine engine)
    {
        var node = (XmlCharacterData)nodeList.Current;

        Debug.Assert(node.Value != null);
        ProcessString(node, node.Value, engine);
    }

    /// <summary>Processes the string.</summary>
    /// <param name="node">The node.</param>
    /// <param name="value">The value.</param>
    /// <param name="engine">The context.</param>
    public static void ProcessString(XmlNode node, string value, IXmlProcessorEngine engine)
    {
        var fragment = CreateFragment(node);

        Match match;
        var pos = 0;
        while ((match = GetPropertyValidationRegex().Match(value, pos)).Success)
        {
            if (pos < match.Index)
            {
                AppendChild(fragment, value.Substring(pos, match.Index - pos));
            }

            var propRef = match.Groups[1].Value; // #!{ propKey }
            var propKey = match.Groups[2].Value; // propKey

            {
            }
            var prop = engine.GetProperty(propKey);

            if (prop != null)
            {
                // When node has a parentNode (not an attribute)
                // we copy any attributes for the property into the parentNode
                if (node.ParentNode != null)
                {
                    MoveAttributes((XmlElement)node.ParentNode, prop);
                }

                AppendChild(fragment, prop.ChildNodes);
            }
            else if (IsRequiredProperty(propRef))
            {
                throw new XmlProcessorException($"Required configuration property {propKey} not found");
            }

            pos = match.Index + match.Length;
        }

        switch (pos)
        {
            // Appending anything left
            case > 0 when pos < value.Length:
                AppendChild(fragment, value[pos..]);
                break;
            // we only process when there was at least one match
            // even when the fragment contents is empty since
            // that could mean that there was a match but the property
            // reference was a silent property
            case <= 0:
                return;
        }

        if (node.NodeType == XmlNodeType.Attribute)
        {
            node.Value = fragment.InnerText.Trim();
        }
        else
        {
            Debug.Assert(node.ParentNode != null);
            ReplaceNode(node.ParentNode, fragment, node);
        }
    }

    private static bool IsRequiredProperty(string propRef)
    {
        return propRef.StartsWith("#{");
    }

    private static void MoveAttributes(XmlElement targetElement, XmlElement srcElement)
    {
        for (var i = srcElement.Attributes.Count - 1; i > -1; i--)
        {
            var importedAttr = ImportNode(targetElement, srcElement.Attributes[i]) as XmlAttribute;
            Debug.Assert(importedAttr != null);
            targetElement.Attributes.Append(importedAttr);
        }
    }

    [GeneratedRegex(@"(\#!?\{\s*((?:\w|\.)+)\s*\})", RegexOptions.Compiled)]
    private static partial Regex GetPropertyValidationRegex();
}