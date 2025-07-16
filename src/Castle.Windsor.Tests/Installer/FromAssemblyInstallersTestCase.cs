// Copyright 2004-2022 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Installer;

using System;
using System.IO;
using System.Reflection;

using Castle.MicroKernel.Registration;
using Castle.Windsor.Installer;

public class FromAssemblyInstallersTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Can_install_from_assembly_by_assembly()
	{
		Container.Install(FromAssembly.Instance(typeof(FromAssemblyInstallersTestCase).GetTypeInfo().Assembly));
		Container.Resolve<object>("Customer-by-CustomerInstaller");
	}

	[Fact]
	public void Can_install_from_assembly_by_directory_simple()
	{
		var location = AppContext.BaseDirectory;
		Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location)));
		Container.Resolve<object>("Customer-by-CustomerInstaller");
	}

	[Fact]
	public void Can_install_from_assembly_by_name()
	{
		Container.Install(FromAssembly.Named("Castle.Windsor.Tests"));
	}

	[Fact]
	public void Can_install_from_assembly_by_type()
	{
		Container.Install(FromAssembly.Containing(GetType()));
	}

	[Fact]
	public void Can_install_from_assembly_by_application()
	{
		Container.Install(FromAssembly.InThisApplication(GetCurrentAssembly(), new FilterAssembliesInstallerFactory(t => t.GetTypeInfo().Assembly != typeof(IWindsorInstaller).GetTypeInfo().Assembly)));
	}

	[Fact]
	public void Can_install_from_assembly_by_type_generic()
	{
		Container.Install(FromAssembly.Containing<FromAssemblyInstallersTestCase>());
	}

	[Fact]
	public void Can_install_from_calling_assembly1()
	{
		Container.Install(FromAssembly.Instance(GetCurrentAssembly()));
	}

	[Fact]
	public void Can_install_from_calling_assembly2()
	{
		Container.Install(FromAssembly.This());
	}

	[Fact]
	public void Install_from_assembly_by_directory_ignores_non_existing_path()
	{
		var location = Path.Combine(AppContext.BaseDirectory, Guid.NewGuid().ToString("N"));

		Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location)));

		Assert.Empty(Container.Kernel.GraphNodes);
	}

	[Fact]
	public void Install_from_assembly_by_directory_executes_assembly_condition()
	{
		var location = AppContext.BaseDirectory;
		var called = false;
		Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).FilterByAssembly(a =>
		{
			called = true;
			return true;
		})));

		Assert.True(called);
		Assert.True(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
	}

	[Fact]
	public void Install_from_assembly_by_directory_empty_name_searches_currentDirectory()
	{
		var called = false;
		Container.Install(FromAssembly.InDirectory(new AssemblyFilter(string.Empty).FilterByAssembly(a =>
		{
			called = true;
			return true;
		})));

		Assert.True(called);
		Assert.True(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
	}

	[Fact]
	public void Install_from_assembly_by_directory_executes_name_condition()
	{
		var location = AppContext.BaseDirectory;
		var byNameCalled = false;
		Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).FilterByName(a =>
		{
			byNameCalled = true;
			return true;
		})));

		Assert.True(byNameCalled);
		Assert.True(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
	}

	[Fact]
	public void Install_from_assembly_by_directory_obeys_assembly_condition()
	{
		var location = AppContext.BaseDirectory;
		var called = false;
		Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).FilterByAssembly(a =>
		{
			called = true;
			return false;
		})));

		Assert.True(called);
		Assert.False(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
	}

	[Fact]
	public void Install_from_assembly_by_directory_obeys_name_condition()
	{
		var location = AppContext.BaseDirectory;
		var byNameCalled = false;
		Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).FilterByName(a =>
		{
			byNameCalled = true;
			return false;
		})));

		Assert.True(byNameCalled);
		Assert.False(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
	}

	[Fact]
	public void Install_from_assembly_by_directory_with_fake_key_as_string_does_not_install()
	{
		var location = AppContext.BaseDirectory;

		Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).WithKeyToken("1234123412341234")));
		Assert.False(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
	}

	[Fact]
	public void Install_from_assembly_by_directory_with_key_as_string_installs()
	{
		var location = AppContext.BaseDirectory;

		var fullName = GetType().GetTypeInfo().Assembly.FullName;
		var index = fullName.IndexOf("PublicKeyToken=");
		if (index == -1) Assert.Fail("Assembly is not signed so no way to test this.");
		var publicKeyToken = fullName.Substring(index + "PublicKeyToken=".Length, 16);
		Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).WithKeyToken(publicKeyToken)));
		Assert.True(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
	}

	[Fact]
	public void Install_from_assembly_by_directory_with_key_installs()
	{
		var location = AppContext.BaseDirectory;

		var publicKeyToken = GetType().GetTypeInfo().Assembly.GetName().GetPublicKeyToken();
		if (publicKeyToken == null || publicKeyToken.Length == 0) Assert.Fail("Assembly is not signed so no way to test this.");

		Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).WithKeyToken(GetType())));
		Assert.True(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
	}

	[Fact]
	public void Install_from_assembly_by_directory_with_mscorlib_key_does_not_install()
	{
		var location = AppContext.BaseDirectory;

		var publicKeyToken = GetType().GetTypeInfo().Assembly.GetName().GetPublicKeyToken();
		if (publicKeyToken == null || publicKeyToken.Length == 0) Assert.Fail("Assembly is not signed so no way to test this.");

		Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).WithKeyToken<object>()));
		Assert.False(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
	}
}