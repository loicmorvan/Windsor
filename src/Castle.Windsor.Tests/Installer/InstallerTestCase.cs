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

using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.XmlFiles;
using Castle.Windsor.Windsor;
using Castle.Windsor.Windsor.Configuration.Interpreters;

namespace Castle.Windsor.Tests.Installer;

public class InstallerTestCase : AbstractContainerTestCase
{
	[Fact]
	public void InstallCalcService()
	{
		var container = new WindsorContainer(new XmlInterpreter(Xml.Embedded("installerconfig.xml")));

		Assert.True(container.Kernel.HasComponent(typeof(ICalcService)));
		Assert.True(container.Kernel.HasComponent("calcservice"));
	}

	[Fact]
	public void InstallChildContainer()
	{
		var container = new WindsorContainer(new XmlInterpreter(Xml.Embedded("installerconfig.xml")));
		var child1 = container.GetChildContainer("child1");

		Assert.NotNull(child1);
		Assert.Equal(child1.Parent, container);
		Assert.True(child1.Kernel.HasComponent(typeof(ICalcService)));
		Assert.True(child1.Kernel.HasComponent("child_calcservice"));

		var calcservice = container.Resolve<ICalcService>("calcservice");
		var child_calcservice = child1.Resolve<ICalcService>();
		Assert.NotEqual(calcservice, child_calcservice);
	}
}