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

namespace Castle.Windsor.Tests.Configuration2.Properties;

using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Windsor;
using Castle.Windsor.Windsor.Configuration.Interpreters;

public class PropertiesTestCase
{
	private IWindsorContainer container;

	[Fact]
	public void CorrectEval()
	{
		container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/Properties/config_with_properties.xml"));

		AssertConfiguration();
	}

	[Fact]
	public void MissingProperties()
	{
		Assert.Throws<ConfigurationProcessingException>(() =>
			container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/Properties/config_with_missing_properties.xml")));
	}

	[Fact]
	public void PropertiesAndDefines()
	{
		container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/Properties/config_with_properties_and_defines.xml"));

		AssertConfiguration();
	}

	[Fact]
	public void PropertiesAndDefines2()
	{
		container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/Properties/config_with_properties_and_defines2.xml"));

		AssertConfiguration();
	}

	[Fact]
	public void PropertiesAndIncludes()
	{
		container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/Properties/config_with_properties_and_includes.xml"));

		AssertConfiguration();
	}

	[Fact]
	public void PropertiesWithinProperties()
	{
		container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/Properties/properties_using_properties.xml"));

		AssertConfiguration();
	}

	[Fact]
	public void SilentProperties()
	{
		container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/Properties/config_with_silent_properties.xml"));

		var store = container.Kernel.ConfigurationStore;

		Assert.Single(store.GetFacilities());
		Assert.Single(store.GetComponents());

		var config = store.GetFacilityConfiguration(typeof(NoopFacility).FullName);
		var childItem = config.Children["param1"];
		Assert.NotNull(childItem);
		Assert.Equal("prop1 value", childItem.Value);
		Assert.Equal("", childItem.Attributes["attr"]);

		config = store.GetComponentConfiguration("component1");
		childItem = config.Children["param1"];
		Assert.NotNull(childItem);
		Assert.Null(childItem.Value);
		Assert.Equal("prop1 value", childItem.Attributes["attr"]);
	}

	private void AssertConfiguration()
	{
		var store = container.Kernel.ConfigurationStore;

		Assert.Equal(3, store.GetFacilities().Length);
		Assert.Equal(2, store.GetComponents().Length);

		var config = store.GetFacilityConfiguration(typeof(NoopFacility).FullName);
		var childItem = config.Children["item"];
		Assert.NotNull(childItem);
		Assert.Equal("prop1 value", childItem.Value);

		config = store.GetFacilityConfiguration(typeof(Noop2Facility).FullName);
		Assert.NotNull(config);
		childItem = config.Children["item"];
		Assert.NotNull(childItem);
		Assert.Equal("prop2 value", childItem.Attributes["value"]);
		Assert.Null(childItem.Value);

		config = store.GetFacilityConfiguration(typeof(HiperFacility).FullName);
		Assert.NotNull(config);
		Assert.Equal(3, config.Children.Count);

		childItem = config.Children["param1"];
		Assert.NotNull(childItem);
		Assert.Equal("prop2 value", childItem.Value);
		Assert.Equal("prop1 value", childItem.Attributes["attr"]);

		childItem = config.Children["param2"];
		Assert.NotNull(childItem);
		Assert.Equal("prop1 value", childItem.Value);
		Assert.Equal("prop2 value", childItem.Attributes["attr"]);

		childItem = config.Children["param3"];
		Assert.NotNull(childItem);
		Assert.Equal("param3 attr", childItem.Attributes["attr"]);

		childItem = childItem.Children["value"];
		Assert.NotNull(childItem);
		Assert.Equal("param3 value", childItem.Value);
		Assert.Equal("param3 value attr", childItem.Attributes["attr"]);

		config = store.GetComponentConfiguration("component1");
		childItem = config.Children["item"];
		Assert.NotNull(childItem);
		Assert.Equal("prop1 value", childItem.Value);

		config = store.GetComponentConfiguration("component2");
		childItem = config.Children["item"];
		Assert.NotNull(childItem);
		Assert.Equal("prop2 value", childItem.Attributes["value"]);
	}
}