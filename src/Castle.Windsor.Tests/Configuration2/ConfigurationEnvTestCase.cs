// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests.Configuration2;

public class ConfigurationEnvTestCase
{
	[Fact]
	public void AssertDefineIsSetBasedOnEnvironmentInformation()
	{
		var configPath = ConfigHelper.ResolveConfigPath("Configuration2/env_config.xml");
		var container = new WindsorContainer(new XmlInterpreter(configPath), new CustomEnv(true));

		var prop = container.Resolve<ComponentWithStringProperty>("component");

		Assert.Equal("John Doe", prop.Name);

		container = new WindsorContainer(new XmlInterpreter(configPath), new CustomEnv(false));

		prop = container.Resolve<ComponentWithStringProperty>("component");

		Assert.Equal("Hammett", prop.Name);
	}
}

internal class CustomEnv(bool isDevelopment) : IEnvironmentInfo
{
	public string GetEnvironmentName()
	{
		return isDevelopment ? "devel" : "test";
	}
}