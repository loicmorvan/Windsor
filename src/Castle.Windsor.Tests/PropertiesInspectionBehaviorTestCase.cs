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

namespace Castle.Windsor.Tests;

using Castle.Windsor.MicroKernel.SubSystems.Conversion;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.XmlFiles;
using Castle.Windsor.Windsor;
using Castle.Windsor.Windsor.Configuration.Interpreters;

public class PropertiesInspectionBehaviorTestCase
{
	[Fact]
	public void InvalidOption()
	{
		var expectedMessage =
			"Error on properties inspection. Could not convert the inspectionBehavior attribute value into an expected enum value. Value found is 'Invalid' while possible values are 'Undefined, None, All, DeclaredOnly'";
		var exception = Assert.Throws<ConverterException>(() =>
			new WindsorContainer(new XmlInterpreter(Xml.Embedded("propertyInspectionBehaviorInvalid.xml"))));
		Assert.Equal(exception.Message, expectedMessage);
	}

	[Fact]
	public void PropertiesInspectionTestCase()
	{
		var container = new WindsorContainer(new XmlInterpreter(Xml.Embedded("propertyInspectionBehavior.xml")));

		var comp = container.Resolve<ExtendedComponentWithProperties>("comp1");
		Assert.Null(comp.Prop1);
		Assert.Equal(0, comp.Prop2);
		Assert.Equal(0, comp.Prop3);

		comp = container.Resolve<ExtendedComponentWithProperties>("comp2"); // All
		Assert.NotNull(comp.Prop1);
		Assert.Equal(1, comp.Prop2);
		Assert.Equal(2, comp.Prop3);

		comp = container.Resolve<ExtendedComponentWithProperties>("comp3"); // DeclaredOnly
		Assert.Null(comp.Prop1);
		Assert.Equal(0, comp.Prop2);
		Assert.Equal(2, comp.Prop3);
	}
}