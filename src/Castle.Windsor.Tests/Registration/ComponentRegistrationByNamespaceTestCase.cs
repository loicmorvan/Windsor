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

namespace CastleTests.Registration
{
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;

	using RootNamespace;

	public class ComponentRegistrationByNamespaceTestCase : AbstractContainerTestCase
	{
		private int ComponentsCount()
		{
			return Components().Length;
		}

		private IHandler[] Components()
		{
			return Kernel.GetAssignableHandlers(typeof(object));
		}

		[Fact]
		public void Registreting_by_namespace_no_subnamespaces()
		{
			Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(Component.IsInNamespace("RootNamespace")));

			Assert.Equal(1, ComponentsCount());
		}

		[Fact]
		public void Registreting_by_namespace_no_subnamespaces_by_type_generic_short()
		{
			Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).InSameNamespaceAs<RootComponent>());

			Assert.Equal(1, ComponentsCount());
		}

		[Fact]
		public void Registreting_by_namespace_no_subnamespaces_by_type_short()
		{
			Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).InSameNamespaceAs(typeof(RootComponent)));

			Assert.Equal(1, ComponentsCount());
		}

		[Fact]
		public void Registreting_by_namespace_no_subnamespaces_short()
		{
			Kernel.Register(Classes.FromAssembly(GetCurrentAssembly()).InNamespace("RootNamespace"));

			Assert.Equal(1, ComponentsCount());
		}

		[Fact]
		public void Registreting_by_namespace_with_subnamespaces()
		{
			Kernel.Register(
				Classes.FromAssembly(GetCurrentAssembly())
					.Where(Component.IsInNamespace("RootNamespace", true)));

			Assert.Equal(2, ComponentsCount());
		}

		[Fact]
		public void Registreting_by_namespace_with_subnamespaces_by_type_generic_short()
		{
			Kernel.Register(
				Classes.FromAssembly(GetCurrentAssembly()).InSameNamespaceAs<RootComponent>(true));

			Assert.Equal(2, ComponentsCount());
		}

		[Fact]
		public void Registreting_by_namespace_with_subnamespaces_by_type_short()
		{
			Kernel.Register(
				Classes.FromAssembly(GetCurrentAssembly()).InSameNamespaceAs(typeof(RootComponent), true));

			Assert.Equal(2, ComponentsCount());
		}

		[Fact]
		public void Registreting_by_namespace_with_subnamespaces_properly_filters_out_namespaces_that_have_common_prefix()
		{
			Kernel.Register(
				Classes.FromAssembly(GetCurrentAssembly())
					.Where(Component.IsInNamespace("RootNamespace", true)));

			Assert.DoesNotContain(Components(), c => c.ComponentModel.Services.Any(s => s.Namespace == "RootNamespaceEx"));
		}

		[Fact]
		public void Registreting_by_namespace_with_subnamespaces_short()
		{
			Kernel.Register(
				Classes.FromAssembly(GetCurrentAssembly()).InNamespace("RootNamespace", true));

			Assert.Equal(2, ComponentsCount());
		}
	}
}

namespace RootNamespace
{
	public class RootComponent;
}

namespace RootNamespaceEx
{
	public class RootComponentEx;
}

namespace RootNamespace.Sub
{
	public class SubComponent;
}