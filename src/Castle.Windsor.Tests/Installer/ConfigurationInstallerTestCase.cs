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
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.SubSystems.Configuration;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.XmlFiles;
using Castle.Windsor.Windsor;
using Castle.Windsor.Windsor.Installer;

namespace Castle.Windsor.Tests.Installer;

public class ConfigurationInstallerTestCase : AbstractContainerTestCase
{
#if FEATURE_SYSTEM_CONFIGURATION
		[Fact]
		public void Can_reference_components_from_app_config_in_component_node()
		{
			Container.Install(Configuration.FromAppConfig());

			var item = Container.Resolve<ClassWithArguments>();
			Assert.Equal("a string", item.Arg1);
			Assert.Equal(42, item.Arg2);
		}

		[Fact]
		public void InstallComponents_FromAppConfig_ComponentsInstalled()
		{
			Container.Install(Configuration.FromAppConfig());

			Assert.True(Container.Kernel.HasComponent(typeof(ICalcService)));
			Assert.True(Container.Kernel.HasComponent("calcservice"));
		}

		[Fact]
		public void InstallComponents_FromMultiple_ComponentsInstalled()
		{
			Container.Install(
				Configuration.FromAppConfig(),
				Configuration.FromXml(Xml.Embedded("ignoreprop.xml")),
				Configuration.FromXml(Xml.Embedded("robotwireconfig.xml"))
				);

			Assert.True(Container.Kernel.HasComponent(typeof(ICalcService)));
			Assert.True(Container.Kernel.HasComponent("calcservice"));
			Assert.True(Container.Kernel.HasComponent(typeof(ClassWithDoNotWireProperties)));
			Assert.True(Container.Kernel.HasComponent("server"));
			Assert.True(Container.Kernel.HasComponent(typeof(Robot)));
			Assert.True(Container.Kernel.HasComponent("robot"));
		}
#endif

	[Fact]
	public void InstallComponents_FromXmlFileWithEnvironment_ComponentsInstalled()
	{
		Container.Install(
			Configuration.FromXmlFile(
					ConfigHelper.ResolveConfigPath("Configuration2/env_config.xml"))
				.Environment("devel")
		);

		var prop = Container.Resolve<ComponentWithStringProperty>("component");

		Assert.Equal("John Doe", prop.Name);
	}

	[Fact]
	public void InstallComponents_FromXmlFile_ComponentsInstalled()
	{
		Container.Install(
			Configuration.FromXml(Xml.Embedded("installerconfig.xml")));

		Assert.True(Container.Kernel.HasComponent(typeof(ICalcService)));
		Assert.True(Container.Kernel.HasComponent("calcservice"));
	}

	[Fact]
	public void InstallComponents_FromXmlFile_first_and_from_code()
	{
		Container.Install(
			Configuration.FromXml(Xml.Embedded("justConfiguration.xml")),
			new Installer(c => c.Register(Component.For<ICamera>()
				.ImplementedBy<Camera>()
				.Named("camera"))));

		var camera = Container.Resolve<ICamera>();
		Assert.Equal("from configuration", camera.Name);
	}
}

internal class Installer : IWindsorInstaller
{
	private readonly Action<IWindsorContainer> install;

	public Installer(Action<IWindsorContainer> install)
	{
		this.install = install;
	}

	public void Install(IWindsorContainer container, IConfigurationStore store)
	{
		install(container);
	}
}