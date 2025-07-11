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

using System.Linq;
using Castle.Core;
using Castle.Facilities.Startable;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Generics;
using Castle.Windsor.Tests.XmlFiles;

namespace Castle.Windsor.Tests.Windsor.Tests;

public class XmlConfigStructureTestCase : AbstractContainerTestCase
{
	private IWindsorInstaller FromFile(string fileName)
	{
		var file = Xml.Embedded(fileName);
		return Castle.Windsor.Installer.Configuration.FromXml(file);
	}

	[Fact]
	public void Bound_lifestyle_can_be_specify_via_type_only()
	{
		Container.Install(FromFile("BoundLifestyle.xml"));
		var handler = Kernel.GetHandler(typeof(A));

		Assert.NotNull(handler);
		Assert.Equal(LifestyleType.Bound, handler.ComponentModel.LifestyleType);

		var a = Container.Resolve<GenericA<A>>();
		Assert.Same(a.Item, a.B.Item);
	}

	[Fact]
	public void Scoped_lifestyle_can_be_specified_via_type_only()
	{
		Container.Install(FromFile("ScopedLifestyleImplicit.xml"));
		var handler = Kernel.GetHandler(typeof(A));

		Assert.NotNull(handler);
		Assert.Equal(LifestyleType.Scoped, handler.ComponentModel.LifestyleType);

		using (Container.BeginScope())
		{
			var a1 = Container.Resolve<A>();
			var a2 = Container.Resolve<A>();
			Assert.Same(a1, a2);
		}
	}

	[Fact]
	public void Scoped_lifestyle_can_be_specified_in_Xml()
	{
		Container.Install(FromFile("ScopedLifestyle.xml"));
		var handler = Kernel.GetHandler(typeof(A));

		Assert.NotNull(handler);
		Assert.Equal(LifestyleType.Scoped, handler.ComponentModel.LifestyleType);

		using (Container.BeginScope())
		{
			var a1 = Container.Resolve<A>();
			var a2 = Container.Resolve<A>();
			Assert.Same(a1, a2);
		}
	}

	[Fact]
	public void Custom_lifestyle_can_be_specify_via_type_only()
	{
		Container.Install(FromFile("CustomLifestyle.xml"));
		var handler = Kernel.GetHandler(typeof(A));

		Assert.NotNull(handler);
		Assert.Equal(LifestyleType.Custom, handler.ComponentModel.LifestyleType);
		Assert.Equal(typeof(CustomLifestyleManager), handler.ComponentModel.CustomLifestyle);
	}

	[Fact]
	public void Id_is_not_required_for_component_if_type_is_specified()
	{
		Container.Install(FromFile("componentWithoutId.xml"));
		Kernel.Resolve<A>();
	}

	[Fact]
	public void Id_is_not_required_for_facility_if_type_is_specified()
	{
		Container.Install(FromFile("facilityWithoutId.xml"));
		var facilities = Kernel.GetFacilities();
		Assert.NotEmpty(facilities);
		Assert.IsType<StartableFacility>(facilities.Single());
	}

	[Fact]
	[Bug("IoC-103")]
	public void Invalid_nodes_are_reported_via_exception()
	{
		var e =
			Assert.Throws<ConfigurationProcessingException>(() => Container.Install(FromFile("IOC-103.xml")));

		var expected =
			@"Configuration parser encountered <aze>, but it was expecting to find <installers>, <facilities> or <components>. There might be either a typo on <aze> or you might have forgotten to nest it properly.";
		Assert.Equal(expected, e.Message);
	}
}