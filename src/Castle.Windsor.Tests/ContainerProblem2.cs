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

using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Windsor;

// ReSharper disable UnusedParameter.Local

namespace Castle.Windsor.Tests;

public class ContainerProblem2
{
	[Fact]
	public void CausesStackOverflow()
	{
		IWindsorContainer container = new WindsorContainer();

		container.Register(Component.For(typeof(IS)).ImplementedBy<Bs>().Named("BS"));
		container.Register(Component.For(typeof(IC)).ImplementedBy<CImpl>().Named("C"));
		container.Register(Component.For(typeof(IWm)).ImplementedBy<Wm>().Named("WM"));
		container.Register(Component.For(typeof(ISp)).ImplementedBy<Sp>().Named("SP"));

		//TODO: dead code - why is it here?
		// ComponentModel model = new ComponentModel("R", typeof(R), typeof(R));
		// model.LifestyleType = LifestyleType.Custom;
		// model.CustomLifestyle = typeof(PerThreadLifestyleManager);

		// container.Kernel.AddCustomComponent(model);
		// container.Kernel.AddComponent("R", typeof(R), LifestyleType.Thread);
		container.Kernel.Register(Component.For(typeof(R)).Named("R"));

		var c = container.Resolve<IC>("C");
		Assert.NotNull(c);
	}
}