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
using Castle.Core.Resource;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.XmlFiles;

namespace Castle.Windsor.Tests;

public class CreatingContainerTestCase
{
#if FEATURE_SYSTEM_CONFIGURATION
		[Fact]
		public void With_config_section()
		{
			var sectionName = "config://castle/";//trailing slash is required

			var uri = new CustomUri(sectionName);

			Assert.Equal("config", uri.Scheme);
			Assert.Equal("castle", uri.Host);

			var container = new WindsorContainer(sectionName);

			container.Resolve<ICalcService>("calcservice");
		}
#endif

	[Fact]
	public void With_configuration_file()
	{
		var filePath = Xml.FilePath("hasFileIncludes.xml");

		Assert.True(new Uri(filePath).IsFile);

		var container = new WindsorContainer(filePath);

		Assert.Equal(2, container.Kernel.GetFacilities().Length);
	}

	[Fact]
	public void With_configuration_file_relative()
	{
		var filePath = "XmlFiles/hasFileIncludes.xml";

		var container = new WindsorContainer(filePath);

		Assert.Equal(2, container.Kernel.GetFacilities().Length);
	}

	[Fact]
	public void With_embedded_xml()
	{
		var resourcePath = Xml.EmbeddedPath("componentWithoutId.xml");

		Assert.True(new CustomUri(resourcePath).IsAssembly);

		var container = new WindsorContainer(resourcePath);

		container.Resolve<A>();
	}
}