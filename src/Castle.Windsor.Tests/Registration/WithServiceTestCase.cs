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
using System.Reflection;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests.Registration;

public class WithServiceTestCase : AbstractContainerTestCase
{
	[Fact]
	public void AllInterfaces_uses_all_implemented_interfaces()
	{
		Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(t => t == typeof(TwoInterfacesImpl))
			.WithService.AllInterfaces());
		var services = Kernel.GetAssignableHandlers(typeof(object)).Single().ComponentModel.Services.ToArray();
		Assert.Equal(2, services.Length);
		Assert.Contains(services, s => s == typeof(ICommon));
		Assert.Contains(services, s => s == typeof(ICommon2));
	}

	[Fact]
	public void AllInterfaces_uses_single_interface()
	{
		Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(t => t == typeof(CommonImpl1)).WithService
			.AllInterfaces());
		var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
		Assert.Equal(typeof(ICommon), handler.ComponentModel.Services.Single());
	}

	[Fact]
	public void AllTypes_not_specified_uses_service_itself()
	{
		Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(t => t == typeof(CommonImpl1)));
		var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
		Assert.Equal(typeof(CommonImpl1), handler.ComponentModel.Services.Single());
	}

	[Fact]
	public void Base_uses_both_types_from_BasedOn_if_implemented_generically_twice()
	{
		// NOTE: This scenario should be tested for not just Base, but also other methods as well?
		Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).BasedOn(typeof(ISimpleGeneric<>)).WithService
			.Base());

		var allHandlers = Kernel.GetAssignableHandlers(typeof(object));
		var aHandlers = Kernel.GetAssignableHandlers(typeof(ISimpleGeneric<A>));
		var bHandlers = Kernel.GetAssignableHandlers(typeof(ISimpleGeneric<B>));

		Assert.NotEmpty(allHandlers);
		Assert.True(allHandlers.All(h => h.ComponentModel.Implementation == typeof(ClosedSimpleGenericOverAAndB)));
		Assert.NotEmpty(aHandlers);
		Assert.NotEmpty(bHandlers);
	}

	[Fact]
	public void Base_uses_type_from_BasedOn()
	{
		Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).BasedOn<ICommon>().WithService.Base());

		var handlers = Kernel.GetAssignableHandlers(typeof(object));

		Assert.NotEmpty(handlers);
		Assert.True(handlers.All(h => h.ComponentModel.Services.Any(s => s == typeof(ICommon))));
	}

	[Fact]
	public void Can_cumulate_services()
	{
		Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(t => t == typeof(TwoInterfacesImpl))
			.WithService.AllInterfaces()
			.WithService.Self());
		var services = Kernel.GetAssignableHandlers(typeof(object)).Single().ComponentModel.Services.ToArray();
		Assert.Equal(3, services.Length);
		Assert.Contains(services, s => s == typeof(ICommon));
		Assert.Contains(services, s => s == typeof(ICommon2));
		Assert.Contains(services, s => s == typeof(TwoInterfacesImpl));
	}

	[Fact]
	public void Can_cumulate_services_without_duplication()
	{
		Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(t => t == typeof(TwoInterfacesImpl))
			.WithService.AllInterfaces()
			.WithService.FirstInterface());
		var handlers = Kernel.GetAssignableHandlers(typeof(object));
		Assert.Single(handlers);
		var handler = handlers.Single();
		Assert.Contains(handler.ComponentModel.Services, s => s == typeof(ICommon));
		Assert.Contains(handler.ComponentModel.Services, s => s == typeof(ICommon2));
	}

	[Fact]
	public void Component_not_specified_uses_service_itself()
	{
		Kernel.Register(Component.For<CommonImpl1>());
		var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
		Assert.Equal(typeof(CommonImpl1), handler.ComponentModel.Services.Single());
	}

	[Fact]
	public void DefaultInterface_can_match_multiple_types()
	{
		Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(t => t == typeof(CommonSub1Impl)).WithService
			.DefaultInterfaces());
		var one = Kernel.Resolve<ICommon>();
		var two = Kernel.Resolve<ICommonSub1>();
		Assert.Same(one, two);
	}

	[Fact]
	public void DefaultInterface_ignores_not_matched_interfaces()
	{
		Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(t => t == typeof(CommonSub2Impl)).WithService
			.DefaultInterfaces());

		var one = Kernel.Resolve<ICommon>();
		var two = Kernel.Resolve<ICommonSub2>();

		Assert.Same(one, two);
		Assert.Throws<ComponentNotFoundException>(() => Kernel.Resolve<ICommonSub1>());
	}

	[Fact]
	public void DefaultInterface_ignores_on_no_interface_matched()
	{
		Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(t => t == typeof(TwoInterfacesImpl))
			.WithService.DefaultInterfaces());
		var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
		Assert.Equal(typeof(TwoInterfacesImpl), handler.ComponentModel.Services.Single());
	}

	[Fact]
	public void DefaultInterface_matches_by_type_name()
	{
		Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(t => t == typeof(CommonImpl1)).WithService
			.DefaultInterfaces());
		var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
		Assert.Equal(typeof(ICommon), handler.ComponentModel.Services.Single());
	}

	[Fact]
	public void FirstInterface_uses_single_interface()
	{
		Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(t => t == typeof(CommonImpl1)).WithService
			.FirstInterface());
		var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
		Assert.Equal(typeof(ICommon), handler.ComponentModel.Services.Single());
	}

	[Fact]
	public void FromInterface_uses_subtypes_of_type_from_BasedOn_but_not_the_type_itself()
	{
		Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).BasedOn<ICommon>().WithService.FromInterface());

		var handlers = Kernel.GetAssignableHandlers(typeof(object));

		Assert.NotEmpty(handlers);
		Assert.True(
			handlers.All(h => typeof(ICommon).GetTypeInfo().IsAssignableFrom(h.ComponentModel.Services.Single())));
		Assert.Contains(handlers, h => typeof(ICommon) != h.ComponentModel.Services.Single());
	}

	[Fact]
	public void Self_uses_service_itself()
	{
		Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(t => t == typeof(CommonImpl1)).WithService
			.Self());
		var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
		Assert.Equal(typeof(CommonImpl1), handler.ComponentModel.Services.Single());
	}
}