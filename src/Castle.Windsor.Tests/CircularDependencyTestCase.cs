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

using System;
using System.Reflection;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.XmlFiles;
using Castle.Windsor.Windsor;
using Castle.Windsor.Windsor.Configuration.Interpreters;
using JetBrains.Annotations;

// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Castle.Windsor.Tests
{
	public class CircularDependencyTestCase : AbstractContainerTestCase
	{
		[Fact]
		public void ShouldNotGetCircularDependencyExceptionWhenResolvingTypeOnItselfWithDifferentModels()
		{
			var container = new WindsorContainer(new XmlInterpreter(Xml.Embedded("IOC-51.xml")));
			Assert.NotNull(container.Resolve<object>("path.fileFinder"));
		}

		[Fact]
		public void ShouldNotSetTheViewControllerProperty()
		{
			Container.Register(Component.For<IController>().ImplementedBy<Controller>().Named("controller"),
				Component.For<IView>().ImplementedBy<View>().Named("view"));
			var controller = Container.Resolve<Controller>("controller");
			Assert.NotNull(controller.View);
			Assert.Null(controller.View.Controller);
		}

		[Fact]
		public void Should_not_try_to_instantiate_singletons_twice_when_circular_dependency()
		{
			SingletonComponent.CtorCallsCount = 0;
			Container.Register(Component.For<SingletonComponent>(),
				Component.For<SingletonDependency>());

			var component = Container.Resolve<SingletonComponent>();
			Assert.NotNull(component.Dependency);
			Assert.Equal(1, SingletonComponent.CtorCallsCount);
		}

		[Fact]
		public void ThrowsACircularDependencyException2()
		{
			Container.Register(Component.For<CompA>().Named("compA"),
				Component.For<CompB>().Named("compB"),
				Component.For<CompC>().Named("compC"),
				Component.For<CompD>().Named("compD"));

			var exception =
				Assert.Throws<CircularDependencyException>(() => Container.Resolve<CompA>("compA"));
			var expectedMessage =
				string.Format(
					"Dependency cycle has been detected when trying to resolve component 'compA'.{0}The resolution tree that resulted in the cycle is the following:{0}Component 'compA' resolved as dependency of{0}	component 'compD' resolved as dependency of{0}	component 'compC' resolved as dependency of{0}	component 'compB' resolved as dependency of{0}	component 'compA' which is the root component being resolved.{0}",
					Environment.NewLine);
			Assert.Equal(expectedMessage, exception.Message);
		}
	}

	[Singleton]
	[UsedImplicitly]
	public class SingletonComponent
	{
		public static int CtorCallsCount;

		public SingletonComponent()
		{
			CtorCallsCount++;
		}

		public SingletonDependency Dependency { get; set; }
	}

	[Singleton]
	[UsedImplicitly]
	public class SingletonPropertyComponent
	{
		public static int CtorCallsCount;

		public SingletonPropertyComponent()
		{
			CtorCallsCount++;
		}

		public SingletonPropertyDependency Dependency { get; set; }
	}

	[Singleton]
	[UsedImplicitly]
	public class SingletonDependency
	{
		public SingletonDependency(SingletonComponent c)
		{
		}
	}

	[Singleton]
	[UsedImplicitly]
	public class SingletonPropertyDependency
	{
		public SingletonPropertyComponent Component { get; set; }
	}

	namespace IOC51
	{
		public interface IPathProvider
		{
			string Path { get; }
		}

		public class AssemblyPath : IPathProvider
		{
			public string Path
			{
				get
				{
					var uriPath = new Uri(typeof(AssemblyPath).GetTypeInfo().Assembly.Location);
					return uriPath.LocalPath;
				}
			}
		}

		public class RelativeFilePath(IPathProvider basePathProvider, string extensionsPath) : IPathProvider
		{
			public string Path { get; } = System.IO.Path.Combine(basePathProvider.Path + "\\", extensionsPath);
		}
	}
}