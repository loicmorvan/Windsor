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

using System;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;

namespace Castle.Windsor.Tests.Bugs;

public class IoC117
{
	[Fact]
	public void Public_property_with_Protected_setter_causes_Object_Reference_exception()
	{
		IKernel kernel = new DefaultKernel();

		kernel.Register(Component.For<Presenter>());
		kernel.Register(Component.For<View>());

		try
		{
			var p = (Presenter)kernel.Resolve(typeof(Presenter));
			Assert.NotNull(p);
		}
		catch (NullReferenceException)
		{
			Assert.Fail("Should not have thrown a NullReferenceException");
		}
	}
}

public class Presenter
{
	public virtual View View { get; protected set; }
}

public class View;