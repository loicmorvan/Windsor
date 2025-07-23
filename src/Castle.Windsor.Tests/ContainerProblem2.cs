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

using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Windsor;

namespace Castle.Windsor.Tests;

[PerThread]
public class R;

public interface IC
{
	IN N { get; set; }
}

public class CImpl : IC
{
	private R _r;

	public CImpl()
	{
		N = null;
	}

	public R R
	{
		set => _r = value;
	}

	public IN N { get; set; }
}

public interface IN
{
	IS Cs { get; }
}

[Transient]
public class Dn : IN
{
	private ISp _sp;
	private IWm _vm;

	public Dn(IWm vm, ISp sp)
	{
		_vm = vm;
		_sp = sp;
		Cs = new Bs();
	}

	public IS Cs { get; }
}

public interface IWm
{
	void A(IN n);
}

public class Wm : IWm
{
	public void A(IN n)
	{
		//...
	}
}

public interface IS
{
	ISp Sp { get; set; }
}

[Transient]
public class Bs : IS
{
	public ISp Sp { get; set; }
}

public interface ISp
{
	void Save(IS s);
}

public class Sp : ISp
{
	public void Save(IS s)
	{
	}
}

public class ContainerProblem2
{
	[Fact]
	public void CausesStackOverflow()
	{
		IWindsorContainer container = new WindsorContainer();

		container.Register(Component.For(typeof(IS)).ImplementedBy(typeof(Bs)).Named("BS"));
		container.Register(Component.For(typeof(IC)).ImplementedBy(typeof(CImpl)).Named("C"));
		container.Register(Component.For(typeof(IWm)).ImplementedBy(typeof(Wm)).Named("WM"));
		container.Register(Component.For(typeof(ISp)).ImplementedBy(typeof(Sp)).Named("SP"));

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