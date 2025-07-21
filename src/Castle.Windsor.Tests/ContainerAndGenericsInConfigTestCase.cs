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

using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Windsor.Installer;

public class ContainerAndGenericsInConfigTestCase : AbstractContainerTestCase
{
	private IWindsorInstaller FromFile(string fileName)
	{
		return Configuration.FromXmlFile(
			GetFilePath(fileName));
	}

	private string GetFilePath(string fileName)
	{
		return ConfigHelper.ResolveConfigPath("Config/" + fileName);
	}

	[Fact]
	public void Can_build_dependency_chains_of_open_generics()
	{
		Container.Install(FromFile("chainOfResponsibility.config"));

		var resolve = Container.Resolve<IResultFinder<int>>();

		Assert.True(resolve.Finder is DatabaseResultFinder<int>);
		Assert.True(resolve.Finder.Finder is WebServiceResultFinder<int>);
		Assert.True(resolve.Finder.Finder.Finder is FailedResultFinder<int>);
		Assert.True(resolve.Finder.Finder.Finder.Finder == null);
	}

	[Fact]
	public void Can_resolve_closed_generic_service()
	{
		Container.Install(FromFile("GenericsConfig.xml"));
		var repos = Container.Resolve<IRepository<int>>("int.repos.generic");

		Assert.IsType<DemoRepository<int>>(repos);
	}

	[Fact]
	public void Can_resolve_closed_generic_service_decorator()
	{
		Container.Install(FromFile("GenericsConfig.xml"));

		var repository = Container.Resolve<IRepository<int>>("int.repos");

		Assert.IsType<LoggingRepositoryDecorator<int>>(repository);
		Assert.IsType<DemoRepository<int>>(((LoggingRepositoryDecorator<int>)repository).inner);
	}

	[Fact]
	public void Can_resolve_closed_generic_service_decorator_with_service_override()
	{
		Container.Install(FromFile("DecoratorConfig.xml"));
		var repos = Container.Resolve<IRepository<int>>();

		Assert.IsType<LoggingRepositoryDecorator<int>>(repos);
		Assert.IsType<DemoRepository<int>>(((LoggingRepositoryDecorator<int>)repos).inner);
		Assert.Equal("second", ((DemoRepository<int>)((LoggingRepositoryDecorator<int>)repos).inner).Name);
	}

	[Fact]
	public void Can_resolve_open_generic_service_with_service_overrides()
	{
		Container.Install(FromFile("ComplexGenericConfig.xml"));

		var repository = Container.Resolve<IRepository<IEmployee>>();
		Assert.IsType<LoggingRepositoryDecorator<IEmployee>>(repository);

		var inner = ((LoggingRepositoryDecorator<IEmployee>)repository).inner;
		Assert.IsType<DemoRepository<IEmployee>>(inner);

		var actualInner = (DemoRepository<IEmployee>)inner;
		Assert.Equal("Generic Repostiory", actualInner.Name);
		Assert.IsType<DictionaryCache<IEmployee>>(actualInner.Cache);
	}

	[Fact]
	public void Closes_generic_dependency_over_correct_type_for_open_generic_components()
	{
		Container.Install(FromFile("GenericDecoratorConfig.xml"));

		var repos = Container.Resolve<IRepository<string>>();
		Assert.IsType<LoggingRepositoryDecorator<string>>(repos);

		Assert.Equal("second", ((DemoRepository<string>)((LoggingRepositoryDecorator<string>)repos).inner).Name);
	}

	[Fact]
	public void Correctly_detects_unresolvable_dependency_on_same_closed_generic_service()
	{
		Container.Install(FromFile("RecursiveDecoratorConfig.xml"));

		var repository = Container.Resolve<IRepository<int>>();

		Assert.Null(((LoggingRepositoryDecorator<int>)repository).inner);
	}

	[Fact]
	public void Correctly_detects_unresolvable_dependency_on_same_open_generic_service()
	{
		Container.Install(FromFile("RecursiveDecoratorConfigOpenGeneric.xml"));

		var repository = Container.Resolve<IRepository<int>>();

		Assert.Null(((LoggingRepositoryDecorator<int>)repository).inner);
	}

	[Fact]
	public void Prefers_closed_service_over_open()
	{
		Container.Install(FromFile("chainOfResponsibility_smart.config"));

		var ofInt = Container.Resolve<IResultFinder<int>>();
		var ofString = Container.Resolve<IResultFinder<string>>();

		Assert.IsType<CacheResultFinder<int>>(ofInt);
		Assert.IsType<DatabaseResultFinder<int>>(ofInt.Finder);
		Assert.IsType<WebServiceResultFinder<int>>(ofInt.Finder.Finder);
		Assert.Null(ofInt.Finder.Finder.Finder);

		Assert.IsType<ResultFinderStringDecorator>(ofString);
		Assert.NotNull(ofString.Finder);
	}

	[Fact]
	public void Prefers_closed_service_over_open_2()
	{
		Container.Install(FromFile("ComplexGenericConfig.xml"));

		var repository = Container.Resolve<IRepository<IReviewer>>();

		Assert.IsType<ReviewerRepository>(repository);
	}

	[Fact]
	public void Prefers_closed_service_over_open_and_uses_default_components_for_dependencies()
	{
		Container.Install(FromFile("ComplexGenericConfig.xml"));

		var repository = Container.Resolve<IRepository<IReviewer>>();

		Assert.IsType<ReviewerRepository>(repository);
		Assert.IsType<DictionaryCache<IReviewer>>(((ReviewerRepository)repository).Cache);
	}

	[Fact]
	public void Prefers_closed_service_over_open_and_uses_service_overrides_for_dependencies()
	{
		Container.Install(FromFile("ComplexGenericConfig.xml"));

		var repository = Container.Resolve<IRepository<IReviewableEmployee>>();

		Assert.IsType<LoggingRepositoryDecorator<IReviewableEmployee>>(repository);

		var inner = ((LoggingRepositoryDecorator<IReviewableEmployee>)repository).inner;
		Assert.IsType<DemoRepository<IReviewableEmployee>>(inner);

		var actualInner = (DemoRepository<IReviewableEmployee>)inner;
		Assert.Equal("Generic Repostiory With No Cache", actualInner.Name);
		Assert.IsType<NullCache<IReviewableEmployee>>(actualInner.Cache);
	}

	[Fact]
	public void Resolves_named_open_generic_service_even_if_closed_version_with_different_name_exists()
	{
		Container.Install(FromFile("ComplexGenericConfig.xml"));

		var repository = Container.Resolve<IRepository<IReviewer>>("generic.repository");

		Assert.IsNotType<ReviewerRepository>(repository);
		Assert.IsType<DemoRepository<IReviewer>>(repository);
	}

	[Fact]
	public void Returns_same_instance_for_open_generic_singleton_service()
	{
		Container.Install(FromFile("GenericDecoratorConfig.xml"));

		var one = Container.Resolve<IRepository<string>>();
		var two = Container.Resolve<IRepository<string>>();

		Assert.Same(one, two);
	}

	[Fact]
	public void Returns_same_instance_for_open_generic_singleton_service_multiple_closed_types()
	{
		Container.Install(FromFile("GenericDecoratorConfig.xml"));

		var one = Container.Resolve<IRepository<string>>();
		var two = Container.Resolve<IRepository<string>>();
		Assert.Same(one, two);

		var three = Container.Resolve<IRepository<int>>();
		var four = Container.Resolve<IRepository<int>>();
		Assert.Same(three, four);
	}
}