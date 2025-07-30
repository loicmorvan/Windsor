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

using System.Xml;
using Castle.Core.Configuration;
using Castle.Core.Configuration.Xml;
using Castle.Core.Resource;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.SubSystems.Configuration;
using Castle.Windsor.MicroKernel.SubSystems.Conversion;
using Castle.Windsor.MicroKernel.SubSystems.Resource;
using Castle.Windsor.Windsor.Configuration.Interpreters.XmlProcessor;

namespace Castle.Windsor.Windsor.Configuration.Interpreters;

/// <summary>
///     Reads the configuration from a XmlFile. Sample structure:
///     <code>
///     &lt;configuration&gt;
///     &lt;facilities&gt;
///     &lt;facility&gt;
///     
///     &lt;/facility&gt;
///     &lt;/facilities&gt;
///   
///     &lt;components&gt;
///     &lt;component id="component1"&gt;
///     
///     &lt;/component&gt;
///     &lt;/components&gt;
///     &lt;/configuration&gt;
///   </code>
/// </summary>
public sealed class XmlInterpreter : AbstractInterpreter
{
    /// <summary>Initializes a new instance of the <see cref="XmlInterpreter" /> class.</summary>
    /// <param name="filename">The filename.</param>
    public XmlInterpreter(string filename) : base(filename)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="XmlInterpreter" /> class.</summary>
    /// <param name="source">The source.</param>
    public XmlInterpreter(IResource source) : base(source)
    {
    }

    public override void ProcessResource(IResource source, IConfigurationStore store, IKernel kernel)
    {
        var resourceSubSystem = kernel.GetSubSystem(SubSystemConstants.ResourceKey) as IResourceSubSystem;
        var processor = new XmlProcessor.XmlProcessor(EnvironmentName, resourceSubSystem);

        try
        {
            var element = processor.Process(source);
            var converter = kernel.GetConversionManager();
            Deserialize(element, store, converter);
        }
        catch (XmlProcessorException e)
        {
            throw new ConfigurationProcessingException("Unable to process xml resource.", e);
        }
    }

    private static void Deserialize(XmlNode section, IConfigurationStore store, IConversionManager converter)
    {
        foreach (XmlNode node in section)
        {
            if (XmlConfigurationDeserializer.IsTextNode(node))
            {
                throw new ConfigurationProcessingException(
                    $"{node.Name} cannot contain text nodes");
            }

            if (node.NodeType == XmlNodeType.Element)
            {
                DeserializeElement(node, store, converter);
            }
        }
    }

    private static void AssertNodeName(XmlNode node, string expectedName)
    {
        if (expectedName.Equals(node.Name))
        {
            return;
        }

        var message = $"Unexpected node under '{expectedName}': Expected '{expectedName}' but found '{node.Name}'";

        throw new ConfigurationProcessingException(message);
    }

    private static void DeserializeComponent(XmlNode node, IConfigurationStore store, IConversionManager converter)
    {
        var config = XmlConfigurationDeserializer.GetDeserializedNode(node);
        var id = config.Attributes["id"];
        if (string.IsNullOrEmpty(id))
        {
            var type = converter.PerformConversion<Type>(config.Attributes["type"]);
            id = ComponentName.DefaultNameFor(type);
            config.Attributes["id"] = id;
            config.Attributes.Add("id-automatic", bool.TrueString);
        }

        AddComponentConfig(id, config, store);
    }

    private static void DeserializeComponents(XmlNodeList nodes, IConfigurationStore store,
        IConversionManager converter)
    {
        foreach (XmlNode node in nodes)
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            AssertNodeName(node, ComponentNodeName);
            DeserializeComponent(node, store, converter);
        }
    }

    private static void DeserializeContainer(XmlNode node, IConfigurationStore store)
    {
        var config = XmlConfigurationDeserializer.GetDeserializedNode(node);
        var newConfig = new MutableConfiguration(config.Name, node.InnerXml);

        // Copy all attributes
        var allKeys = config.Attributes.AllKeys;

        foreach (var key in allKeys)
        {
            newConfig.Attributes.Add(key, config.Attributes[key]);
        }

        // Copy all children
        newConfig.Children.AddRange(config.Children);

        var name = GetRequiredAttributeValue(config, "name");
        AddChildContainerConfig(name, newConfig, store);
    }

    private static void DeserializeContainers(XmlNodeList nodes, IConfigurationStore store)
    {
        foreach (XmlNode node in nodes)
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            AssertNodeName(node, ContainerNodeName);
            DeserializeContainer(node, store);
        }
    }

    private static void DeserializeElement(XmlNode node, IConfigurationStore store, IConversionManager converter)
    {
        switch (node.Name)
        {
            case ContainersNodeName:
                DeserializeContainers(node.ChildNodes, store);
                break;
            case FacilitiesNodeName:
                DeserializeFacilities(node.ChildNodes, store, converter);
                break;
            case InstallersNodeName:
                DeserializeInstallers(node.ChildNodes, store);
                break;
            case ComponentsNodeName:
                DeserializeComponents(node.ChildNodes, store, converter);
                break;
            default:
            {
                var message = string.Format(
                    "Configuration parser encountered <{0}>, but it was expecting to find " +
                    "<{1}>, <{2}> or <{3}>. There might be either a typo on <{0}> or " +
                    "you might have forgotten to nest it properly.",
                    node.Name, InstallersNodeName, FacilitiesNodeName, ComponentsNodeName);
                throw new ConfigurationProcessingException(message);
            }
        }
    }

    private static void DeserializeFacilities(XmlNodeList nodes, IConfigurationStore store,
        IConversionManager converter)
    {
        foreach (XmlNode node in nodes)
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            AssertNodeName(node, FacilityNodeName);
            DeserializeFacility(node, store, converter);
        }
    }

    private static void DeserializeFacility(XmlNode node, IConfigurationStore store, IConversionManager converter)
    {
        var config = XmlConfigurationDeserializer.GetDeserializedNode(node);
        ThrowIfFacilityConfigurationHasIdAttribute(config);
        var typeName = GetRequiredAttributeValue(config, "type");
        var type = converter.PerformConversion<Type>(typeName);
        AddFacilityConfig(type.FullName, config, store);
    }

    private static void DeserializeInstaller(XmlNode node, IConfigurationStore store)
    {
        var config = XmlConfigurationDeserializer.GetDeserializedNode(node);
        var type = config.Attributes["type"];
        var assembly = config.Attributes["assembly"];
        var directory = config.Attributes["directory"];
        var attributesCount = 0;
        if (string.IsNullOrEmpty(type) == false)
        {
            attributesCount++;
        }

        if (string.IsNullOrEmpty(assembly) == false)
        {
            attributesCount++;
        }

        if (string.IsNullOrEmpty(directory) == false)
        {
            attributesCount++;
        }

        if (attributesCount != 1)
        {
            throw new ConfigurationProcessingException(
                "install must have exactly one of the following attributes defined: 'type', 'assembly' or 'directory'.");
        }

        AddInstallerConfig(config, store);
    }

    private static void DeserializeInstallers(XmlNodeList nodes, IConfigurationStore store)
    {
        foreach (XmlNode node in nodes)
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            AssertNodeName(node, InstallNodeName);
            DeserializeInstaller(node, store);
        }
    }

    private static string GetRequiredAttributeValue(IConfiguration configuration, string attributeName)
    {
        var value = configuration.Attributes[attributeName];

        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }

        var message = $"{configuration.Name} elements expects required non blank attribute {attributeName}";

        throw new ConfigurationProcessingException(message);
    }

    private static void ThrowIfFacilityConfigurationHasIdAttribute(IConfiguration config)
    {
        if (config.Attributes["id"] != null)
        {
            throw new ConfigurationProcessingException(
                "The 'id' attribute has been removed from facility configurations, please remove it from your configuration.");
        }
    }
}