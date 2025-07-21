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

namespace Castle.Windsor.Tests.Config;

using System.Linq;

using Castle.Core.Configuration;
using Castle.Core.Resource;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.SubSystems.Configuration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.XmlFiles;
using Castle.Windsor.Windsor;
using Castle.Windsor.Windsor.Configuration.Interpreters;
using Castle.Windsor.Windsor.Installer;

public class ConfigXmlInterpreterTestCase
{
	[Fact]
	public void ComponentIdGetsLoadedFromTheParsedConfiguration()
	{
		var store = new DefaultConfigurationStore();
		var interpreter = new XmlInterpreter(Xml.Embedded("sample_config_with_spaces.xml"));
		IKernel kernel = new DefaultKernel();
		interpreter.ProcessResource(interpreter.Source, store, kernel);

		var container = new WindsorContainer(store);

		var handler = container.Kernel.GetHandler(typeof(ICalcService));
		Assert.Equal(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
	}

	[Fact]
	public void CorrectConfigurationMapping()
	{
		var store = new DefaultConfigurationStore();
		var interpreter = new XmlInterpreter(Xml.Embedded("sample_config.xml"));
		IKernel kernel = new DefaultKernel();
		interpreter.ProcessResource(interpreter.Source, store, kernel);

		var container = new WindsorContainer(store);
		var facility = container.Kernel.GetFacilities().OfType<HiperFacility>().Single();
		Assert.True(facility.Initialized);
	}

	[Fact]
	public void MissingManifestResourceConfiguration()
	{
		var store = new DefaultConfigurationStore();
		var source = new AssemblyResource("assembly://Castle.Windsor.Tests/missing_config.xml");
		IKernel kernel = new DefaultKernel();
		Assert.Throws<ConfigurationProcessingException>(() => new XmlInterpreter(source).ProcessResource(source, store, kernel));
	}

	[Fact]
	public void ProperDeserialization()
	{
		var store = new DefaultConfigurationStore();

		var interpreter = new XmlInterpreter(Xml.Embedded("sample_config_complex.xml"));
		IKernel kernel = new DefaultKernel();
		interpreter.ProcessResource(interpreter.Source, store, kernel);

		Assert.Equal(2, store.GetFacilities().Length);
		Assert.Equal(2, store.GetComponents().Length);
		Assert.Equal(2, store.GetConfigurationForChildContainers().Length);

		var config = store.GetFacilityConfiguration(typeof(DummyFacility).FullName);
		var childItem = config.Children["item"];
		Assert.NotNull(childItem);
		Assert.Equal("value", childItem.Value);

		config = store.GetFacilityConfiguration(typeof(HiperFacility).FullName);
		Assert.NotNull(config);
		Assert.Equal("value within CDATA section", config.Value);

		config = store.GetComponentConfiguration("testidcomponent1");
		childItem = config.Children["item"];
		Assert.NotNull(childItem);
		Assert.Equal("value1", childItem.Value);

		config = store.GetComponentConfiguration("testidcomponent2");
		childItem = config.Children["item"];
		Assert.NotNull(childItem);
		Assert.Equal("value2", childItem.Value);

		config = store.GetChildContainerConfiguration("child1");
		Assert.NotNull(config);
		Assert.Equal("child1", config.Attributes["name"]);
		Assert.Equal("<configuration />", config.Value);

		config = store.GetChildContainerConfiguration("child2");
		Assert.NotNull(config);
		Assert.Equal("child2", config.Attributes["name"]);
		Assert.Equal("<configuration />", config.Value);
	}

	[Fact]
	public void ProperManifestDeserialization()
	{
		var store = new DefaultConfigurationStore();
		var interpreter = new XmlInterpreter(Xml.File("sample_config_complex.xml"));
		IKernel kernel = new DefaultKernel();
		interpreter.ProcessResource(interpreter.Source, store, kernel);

		Assert.Equal(2, store.GetFacilities().Length);
		Assert.Equal(2, store.GetComponents().Length);
		Assert.Equal(2, store.GetConfigurationForChildContainers().Length);

		var config = store.GetFacilityConfiguration(typeof(DummyFacility).FullName);
		var childItem = config.Children["item"];
		Assert.NotNull(childItem);
		Assert.Equal("value", childItem.Value);

		config = store.GetFacilityConfiguration(typeof(HiperFacility).FullName);
		Assert.NotNull(config);
		Assert.Equal("value within CDATA section", config.Value);

		config = store.GetComponentConfiguration("testidcomponent1");
		childItem = config.Children["item"];
		Assert.NotNull(childItem);
		Assert.Equal("value1", childItem.Value);

		config = store.GetComponentConfiguration("testidcomponent2");
		childItem = config.Children["item"];
		Assert.NotNull(childItem);
		Assert.Equal("value2", childItem.Value);

		config = store.GetChildContainerConfiguration("child1");
		Assert.NotNull(config);
		Assert.Equal("child1", config.Attributes["name"]);
		Assert.Equal("<configuration />", config.Value);

		config = store.GetChildContainerConfiguration("child2");
		Assert.NotNull(config);
		Assert.Equal("child2", config.Attributes["name"]);
		Assert.Equal("<configuration />", config.Value);
	}

	[Fact]
	public void ShouldThrowIfIdAttributeIsPresentInFacilityConfig()
	{
		var facilityConfig = Configuration.FromXml(
			new StaticContentResource(
				@"<castle>
<facilities>
<facility id='IAmGone' loggingApi='custom' />
</facilities>
</castle>"));

		Assert.Throws<ConfigurationProcessingException>(() => { new WindsorContainer().Install(facilityConfig); });
	}
}

public class DummyFacility : IFacility
{
	public void Init(IKernel kernel, IConfiguration facilityConfig)
	{
		Assert.NotNull(facilityConfig);
		var childItem = facilityConfig.Children["item"];
		Assert.NotNull(childItem);
		Assert.Equal("value", childItem.Value);
	}

	public void Terminate()
	{
	}
}