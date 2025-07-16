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

namespace CastleTests.Configuration2;

using Castle.MicroKernel.Tests.ClassComponents;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor.Tests;
using Castle.XmlFiles;

public class IncludesTestCase
{
	private IWindsorContainer container;

	[Fact]
	public void AssemblyResourceAndIncludes()
	{
		container = new WindsorContainer(new XmlInterpreter(Xml.Embedded("hasResourceIncludes.xml")));

		AssertConfiguration();
	}

	[Fact]
	public void FileResourceAndIncludes()
	{
		container = new WindsorContainer(new XmlInterpreter(Xml.File("hasFileIncludes.xml")));

		AssertConfiguration();
	}

	[Fact]
	public void FileResourceAndRelativeIncludes()
	{
		container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/config_with_include_relative.xml"));

		AssertConfiguration();
	}

	[Fact]
	public void FileResourceAndRelativeIncludes2()
	{
		container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/config_with_include_relative2.xml"));

		AssertConfiguration();
	}

	private void AssertConfiguration()
	{
		var store = container.Kernel.ConfigurationStore;

		Assert.Equal(2, store.GetFacilities().Length);
		Assert.Equal(2, store.GetComponents().Length);

		var config = store.GetFacilityConfiguration(typeof(NoopFacility).FullName);
		var childItem = config.Children["item"];
		Assert.NotNull(childItem);
		Assert.Equal("value", childItem.Value);

		config = store.GetFacilityConfiguration(typeof(Noop2Facility).FullName);
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
	}
}