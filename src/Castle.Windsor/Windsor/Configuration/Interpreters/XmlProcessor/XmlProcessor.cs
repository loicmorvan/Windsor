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
using Castle.Core.Resource;
using Castle.MicroKernel.SubSystems.Resource;
using Castle.Windsor.Configuration.Interpreters.XmlProcessor.ElementProcessors;

namespace Castle.Windsor.Configuration.Interpreters.XmlProcessor;

/// <summary>Pendent</summary>
public class XmlProcessor
{
	private readonly IXmlProcessorEngine _engine;

	/// <summary>Initializes a new instance of the <see cref="XmlProcessor" /> class.</summary>
	public XmlProcessor() : this(null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="XmlProcessor" /> class.</summary>
	/// <param name="environmentName">Name of the environment.</param>
	/// <param name="resourceSubSystem">The resource sub system.</param>
	public XmlProcessor(string environmentName, IResourceSubSystem resourceSubSystem)
	{
		_engine = new DefaultXmlProcessorEngine(environmentName, resourceSubSystem);
		RegisterProcessors();
	}

	/// <summary>Initializes a new instance of the <see cref="XmlProcessor" /> class.</summary>
	public XmlProcessor(string environmentName)
	{
		_engine = new DefaultXmlProcessorEngine(environmentName);
		RegisterProcessors();
	}

	public XmlNode Process(XmlNode node)
	{
		try
		{
			if (node.NodeType == XmlNodeType.Document) node = (node as XmlDocument).DocumentElement;

			_engine.DispatchProcessAll(new DefaultXmlProcessorNodeList(node));

			return node;
		}
		catch (ConfigurationProcessingException)
		{
			throw;
		}
		catch (Exception ex)
		{
			var message = $"Error processing node {node.Name}, inner content {node.InnerXml}";

			throw new ConfigurationProcessingException(message, ex);
		}
	}

	public XmlNode Process(IResource resource)
	{
		try
		{
			using (resource)
			{
				var doc = new XmlDocument();
				using (var stream = resource.GetStreamReader())
				{
					doc.Load(stream);
				}

				_engine.PushResource(resource);

				var element = Process(doc.DocumentElement);

				_engine.PopResource();

				return element;
			}
		}
		catch (ConfigurationProcessingException)
		{
			throw;
		}
		catch (Exception ex)
		{
			var message = $"Error processing node resource {resource}";

			throw new ConfigurationProcessingException(message, ex);
		}
	}

	protected void AddElementProcessor(Type t)
	{
		_engine.AddNodeProcessor(t);
	}

	protected virtual void RegisterProcessors()
	{
		AddElementProcessor(typeof(IfElementProcessor));
		AddElementProcessor(typeof(DefineElementProcessor));
		AddElementProcessor(typeof(UndefElementProcessor));
		AddElementProcessor(typeof(ChooseElementProcessor));
		AddElementProcessor(typeof(PropertiesElementProcessor));
		AddElementProcessor(typeof(AttributesElementProcessor));
		AddElementProcessor(typeof(IncludeElementProcessor));
		AddElementProcessor(typeof(IfProcessingInstructionProcessor));
		AddElementProcessor(typeof(DefinedProcessingInstructionProcessor));
		AddElementProcessor(typeof(UndefProcessingInstructionProcessor));
		AddElementProcessor(typeof(DefaultTextNodeProcessor));
		AddElementProcessor(typeof(EvalProcessingInstructionProcessor));
		AddElementProcessor(typeof(UsingElementProcessor));
	}
}