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

using System.Linq;

using Castle.MicroKernel.Registration;

using CastleTests;

public class MultiServiceComponentsTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Can_register_handler_forwarding_using_generics_and_resolveAll()
	{
		Container.Register(
			Component.For<IRepository, IRepository<User>>()
				.ImplementedBy<MyRepository>()
		);
		var services = Container.ResolveAll<IRepository<User>>();

		Assert.Single(services);
		Assert.IsType<MyRepository>(services[0]);
	}

	[Fact]
	public void Can_register_handler_forwarding_with_dependencies()
	{
		Container.Register(
			Component.For<IUserRepository, IRepository>()
				.ImplementedBy<MyRepository2>(),
			Component.For<ServiceUsingRepository>(),
			Component.For<User>()
		);

		Container.Resolve<ServiceUsingRepository>();
	}

	[Fact]
	public void Can_register_multiService_component()
	{
		Container.Register(
			Component.For<IUserRepository, IRepository>()
				.ImplementedBy<MyRepository>()
		);

		Assert.Same(
			Container.Resolve<IRepository>(),
			Container.Resolve<IUserRepository>()
		);
	}

	[Fact]
	public void Can_register_several_handler_forwarding()
	{
		Container.Register(
			Component.For<IUserRepository>()
				.Forward<IRepository, IRepository<User>>()
				.ImplementedBy<MyRepository>()
		);

		Assert.Same(
			Container.Resolve<IRepository<User>>(),
			Container.Resolve<IUserRepository>()
		);
		Assert.Same(
			Container.Resolve<IRepository>(),
			Container.Resolve<IUserRepository>()
		);
	}

	[Fact]
	public void Forwarding_main_service_is_ignored()
	{
		Container.Register(
			Component.For<IUserRepository>()
				.Forward<IUserRepository>()
				.ImplementedBy<MyRepository>());

		var allHandlers = Kernel.GetAssignableHandlers(typeof(object));
		Assert.Single(allHandlers);
		Assert.Single(allHandlers.Single().ComponentModel.Services);
	}

	[Fact]
	public void Forwarding_same_service_twice_is_ignored()
	{
		Container.Register(
			Component.For<IUserRepository>()
				.Forward<IRepository>()
				.Forward<IRepository>()
				.ImplementedBy<MyRepository>());

		var allHandlers = Kernel.GetAssignableHandlers(typeof(object));
		Assert.Single(allHandlers);
		Assert.Equal(2, allHandlers.Single().ComponentModel.Services.Count());
	}

	[Fact]
	public void ResolveAll_Will_Only_Resolve_Unique_Handlers()
	{
		Container.Register(
			Component.For<IUserRepository, IRepository>()
				.ImplementedBy<MyRepository>()
		);

		var repos = Container.ResolveAll<IRepository>();
		Assert.Single(repos);
	}

	public interface IRepository;

	public interface IRepository<T> : IRepository;

	public interface IUserRepository : IRepository<User>;

	public class MyRepository : IUserRepository;

	public class User;

	public class MyRepository2 : IUserRepository
	{
		public MyRepository2(User user)
		{
		}
	}

	public class ServiceUsingRepository
	{
		public ServiceUsingRepository(IRepository repos)
		{
		}
	}
}