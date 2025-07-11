// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

using Castle.Facilities.Startable;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests.Bugs;

public class IoC95
{
	[Fact]
	public void AddingComponentToRootKernelWhenChildKernelHasStartableFacility()
	{
		IKernel kernel = new DefaultKernel();
		IKernel childKernel = new DefaultKernel();
		kernel.AddChildKernel(childKernel);
		childKernel.AddFacility(new StartableFacility());
		kernel.Register(Component.For(typeof(A)).Named("string")); // exception here
	}
}