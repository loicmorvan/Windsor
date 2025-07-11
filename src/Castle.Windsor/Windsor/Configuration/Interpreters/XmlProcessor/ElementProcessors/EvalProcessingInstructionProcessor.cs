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

using System;
using System.Xml;

namespace Castle.Windsor.Configuration.Interpreters.XmlProcessor.ElementProcessors;

public class EvalProcessingInstructionProcessor : AbstractXmlNodeProcessor
{
	private static readonly XmlNodeType[] AcceptNodes = [XmlNodeType.ProcessingInstruction];

	public override XmlNodeType[] AcceptNodeTypes => AcceptNodes;

	public override string Name => "eval";

	public override void Process(IXmlProcessorNodeList nodeList, IXmlProcessorEngine engine)
	{
		var node = nodeList.Current as XmlProcessingInstruction;

		var fragment = CreateFragment(node);

		var expression = node.Data;

		// We don't have an expression evaluator right now, so expression will 
		// be just pre-defined literals that we know how to evaluate

		object evaluated = "";

		if (string.Compare(expression, "$basedirectory", true) == 0) evaluated = AppContext.BaseDirectory;

		fragment.AppendChild(node.OwnerDocument.CreateTextNode(evaluated.ToString()));

		ReplaceNode(node.ParentNode, fragment, node);
	}
}