﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

using System.Collections.Generic;
using Castle.Core.Resource;

namespace Castle.Windsor.Tests;

public class GenericListConvenrterTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Can_read_component_with_dictionary_of_lists()
	{
		var xml =
			@"<configuration>
	<components>
		<component service=""IMyObject"" type=""MyObject"">
			<parameters>
				<stuff>
					<dictionary keyType=""System.Int32, mscorlib"" valueType=""System.Collections.Generic.IList`1[[System.String, mscorlib]], mscorlib"">
					<entry key=""0"">
						<list>
						<item>test1</item>
						<item>test2</item>
						</list>
					</entry>
					</dictionary>
				</stuff>
			</parameters>
		</component>
	</components>
</configuration>";

		Container.Install(Castle.Windsor.Installer.Configuration.FromXml(new StaticContentResource(xml)));
		var item = Container.Resolve<IMyObject>();

		Assert.Equal(1, item.Count);
	}
}

public interface IMyObject
{
	int Count { get; }
}

public class MyObject(IDictionary<int, IList<string>> stuff) : IMyObject
{
	protected readonly IDictionary<int, IList<string>> Stuff = stuff;

	public virtual int Count => Stuff.Count;
}