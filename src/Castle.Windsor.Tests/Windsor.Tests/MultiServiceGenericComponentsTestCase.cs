﻿// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
using Castle.MicroKernel;
using Castle.MicroKernel.Handlers;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Generics;

namespace Castle.Windsor.Tests.Windsor.Tests;

public class MultiServiceGenericComponentsTestCase : AbstractContainerTestCase
{
	[Fact]
	public void Closed_generic_component_with_generic_and_non_generic_service__generic_first_resolve_by_key()
	{
		Container.Register(
			Component.For<Generics.IRepository<A>, IARepository>()
				.ImplementedBy<ARepository<B>>()
				.Named("repo")
		);
		Assert.Same(
			Container.Resolve<Generics.IRepository<A>>("repo"),
			Container.Resolve<IARepository>("repo")
		);
	}

	[Fact]
	public void Closed_generic_component_with_generic_and_non_generic_service__generic_first_resolve_by_type()
	{
		Container.Register(
			Component.For<Generics.IRepository<A>, IARepository>()
				.ImplementedBy<ARepository<B>>()
		);
		Assert.Same(
			Container.Resolve<Generics.IRepository<A>>(),
			Container.Resolve<IARepository>()
		);
	}

	[Fact]
	public void Closed_generic_component_with_generic_and_non_generic_service__non_generic_first_resolve_by_key()
	{
		Container.Register(
			Component.For<IARepository, Generics.IRepository<A>>()
				.ImplementedBy<ARepository<B>>()
				.Named("repo")
		);
		Assert.Same(
			Container.Resolve<Generics.IRepository<A>>("repo"),
			Container.Resolve<IARepository>("repo")
		);
	}

	[Fact]
	public void Closed_generic_component_with_generic_and_non_generic_service__non_generic_first_resolve_by_type()
	{
		Container.Register(
			Component.For<IARepository, Generics.IRepository<A>>()
				.ImplementedBy<ARepository<B>>()
		);
		Assert.Same(
			Container.Resolve<Generics.IRepository<A>>(),
			Container.Resolve<IARepository>()
		);
	}

	[Fact]
	public void Non_generic_component_with_generic_and_non_generic_service__generic_first_resolve_by_key()
	{
		Container.Register(
			Component.For<Generics.IRepository<A>, IARepository>()
				.ImplementedBy<ARepository>()
				.Named("repo")
		);
		Assert.Same(
			Container.Resolve<Generics.IRepository<A>>("repo"),
			Container.Resolve<IARepository>("repo")
		);
	}

	[Fact]
	public void Non_generic_component_with_generic_and_non_generic_service__generic_first_resolve_by_type()
	{
		Container.Register(
			Component.For<Generics.IRepository<A>, IARepository>()
				.ImplementedBy<ARepository>()
		);
		Assert.Same(
			Container.Resolve<Generics.IRepository<A>>(),
			Container.Resolve<IARepository>()
		);
	}

	[Fact]
	public void Non_generic_component_with_generic_and_non_generic_service__non_generic_first_resolve_by_key()
	{
		Container.Register(
			Component.For<IARepository, Generics.IRepository<A>>()
				.ImplementedBy<ARepository>()
				.Named("repo")
		);
		Assert.Same(
			Container.Resolve<Generics.IRepository<A>>("repo"),
			Container.Resolve<IARepository>("repo")
		);
	}

	[Fact]
	public void Non_generic_component_with_generic_and_non_generic_service__non_generic_first_resolve_by_type()
	{
		Container.Register(
			Component.For<IARepository, Generics.IRepository<A>>()
				.ImplementedBy<ARepository>()
		);
		Assert.Same(
			Container.Resolve<Generics.IRepository<A>>(),
			Container.Resolve<IARepository>()
		);
	}

	[Fact]
	public void Open_generic_component_with_generic_and_non_generic_service__generic_first_resolve_by_key()
	{
		Container.Register(
			Component.For(typeof(Generics.IRepository<>)).Forward<IRepository>()
				.ImplementedBy(typeof(Repository<>))
				.Named("repo")
		);

		Container.Resolve<Generics.IRepository<A>>("repo");
	}

	[Fact]
	public void
		Open_generic_component_with_generic_and_non_generic_service__generic_first_resolve_by_key_Object_throws_friendly_message()
	{
		Container.Register(
			Component.For(typeof(Generics.IRepository<>)).Forward<IRepository>()
				.ImplementedBy(typeof(Repository<>))
				.Named("repo")
		);

		var exception = Assert.Throws<HandlerException>(() => Container.Resolve<object>("repo", new Arguments()));

		Assert.Equal(
			string.Format(
				"Requested type System.Object has 0 generic parameter(s), whereas component implementation type Castle.Generics.Repository`1[T] requires 1.{0}This means that Windsor does not have enough information to properly create that component for you.{0}You can instruct Windsor which types it should use to close this generic component by supplying an implementation of IGenericImplementationMatchingStrategy.{0}Please consult the documentation for examples of how to do that.",
				Environment.NewLine),
			exception.Message);
	}

	[Fact]
	public void
		Open_generic_component_with_generic_and_non_generic_service__generic_first_resolve_by_key_non_generic_throws_friendly_message()
	{
		Container.Register(
			Component.For(typeof(Generics.IRepository<>)).Forward<IRepository>()
				.ImplementedBy(typeof(Repository<>))
				.Named("repo")
		);

		var exception = Assert.Throws<HandlerException>(() => Container.Resolve<IRepository>("repo"));

		Assert.Equal(
			string.Format(
				"Requested type Castle.Generics.IRepository has 0 generic parameter(s), whereas component implementation type Castle.Generics.Repository`1[T] requires 1.{0}This means that Windsor does not have enough information to properly create that component for you.{0}You can instruct Windsor which types it should use to close this generic component by supplying an implementation of IGenericImplementationMatchingStrategy.{0}Please consult the documentation for examples of how to do that.",
				Environment.NewLine),
			exception.Message);
	}

	[Fact]
	public void Open_generic_component_with_generic_and_non_generic_service__generic_first_resolve_by_type()
	{
		Container.Register(
			Component.For(typeof(Generics.IRepository<>)).Forward<IRepository>()
				.ImplementedBy(typeof(Repository<>))
		);

		Container.Resolve<Generics.IRepository<A>>();
	}

	[Fact]
	public void Open_generic_component_with_generic_and_non_generic_service__non_generic_first_resolve_by_key()
	{
		Container.Register(
			Component.For<IRepository>().Forward(typeof(Generics.IRepository<>))
				.ImplementedBy(typeof(Repository<>))
				.Named("repo")
		);

		Container.Resolve<Generics.IRepository<A>>("repo");
	}

	[Fact]
	public void Open_generic_component_with_generic_and_non_generic_service__non_generic_first_resolve_by_type()
	{
		Container.Register(
			Component.For<IRepository>().Forward(typeof(Generics.IRepository<>))
				.ImplementedBy(typeof(Repository<>))
		);

		Container.Resolve<Generics.IRepository<A>>();
	}
}