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
using System.Threading;
using Castle.Core.Configuration;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Context;
using Castle.Windsor.MicroKernel.Handlers;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Generics;

namespace Castle.Windsor.Tests.Lifestyle;

public class LifestyleManagerTestCase : AbstractContainerTestCase
{
	private IComponent instance3;

	private void TestLifestyleAndSameness(Type componentType, LifestyleType lifestyle, bool overwrite, bool areSame)
	{
		var key = TestHandlersLifestyle(componentType, lifestyle, overwrite);
		TestSameness(key, areSame);
	}

	private void TestLifestyleWithServiceAndSameness(Type componentType, LifestyleType lifestyle, bool overwrite,
		bool areSame)
	{
		var key = TestHandlersLifestyleWithService(componentType, lifestyle, overwrite);
		TestSameness(key, areSame);
	}

	private void TestSameness(string key, bool areSame)
	{
		var one = Kernel.Resolve<IComponent>(key);
		var two = Kernel.Resolve<IComponent>(key);
		if (areSame)
			Assert.Same(one, two);
		else
			Assert.NotSame(one, two);
	}

	private string TestHandlersLifestyle(Type componentType, LifestyleType lifestyle, bool overwrite)
	{
		var key = Guid.NewGuid().ToString();
		Kernel.Register(Component.For(componentType).Named(key).LifeStyle.Is(lifestyle));
		var handler = Kernel.GetHandler(key);
		Assert.Equal(lifestyle, handler.ComponentModel.LifestyleType);
		return key;
	}

	private string TestHandlersLifestyleWithService(Type componentType, LifestyleType lifestyle, bool overwrite)
	{
		var key = Guid.NewGuid().ToString();
		Kernel.Register(Component.For<IComponent>().ImplementedBy(componentType).Named(key).LifeStyle.Is(lifestyle));
		var handler = Kernel.GetHandler(key);
		Assert.Equal(lifestyle, handler.ComponentModel.LifestyleType);
		return key;
	}

	private Property ScopeRoot()
	{
		return Property.ForKey(HandlerExtensionsUtil.ResolveExtensionsKey).Eq(new IResolveExtension[] { new CustomLifestyle_Scope() });
	}

	private void OtherThread()
	{
		var handler = Kernel.GetHandler("a");
		instance3 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;
	}

	[Fact]
	public void BadLifestyleSetProgromatically()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			Kernel.Register(Component.For<IComponent>()
				.ImplementedBy(typeof(TrivialComponent))
				.Named("a")
				.LifeStyle.Is(LifestyleType.Undefined)));
	}

	[Fact]
	public void Custom_lifestyle_provided_via_attribute()
	{
		Kernel.Register(Component.For<IComponent>().ImplementedBy<CustomComponent>());

		var handler = Kernel.GetHandler(typeof(IComponent));
		Assert.Equal(LifestyleType.Custom, handler.ComponentModel.LifestyleType);
		Assert.Equal(typeof(CustomLifestyleManager), handler.ComponentModel.CustomLifestyle);

		var instance = Kernel.Resolve<IComponent>();
		Assert.NotNull(instance);
	}

	[Fact]
	public void Custom_lifestyle_provided_via_attribute_inherited()
	{
		Kernel.Register(Component.For<IComponent>().ImplementedBy<CustomComponentWithCustomLifestyleAttribute>());

		var handler = Kernel.GetHandler(typeof(IComponent));
		Assert.Equal(LifestyleType.Custom, handler.ComponentModel.LifestyleType);
		Assert.Equal(typeof(CustomLifestyleManager), handler.ComponentModel.CustomLifestyle);

		var instance = Kernel.Resolve<IComponent>();
		Assert.NotNull(instance);
	}

	[Fact]
	public void LifestyleSetProgramatically()
	{
		TestHandlersLifestyle(typeof(TrivialComponent), LifestyleType.Transient, false);
		TestHandlersLifestyle(typeof(TrivialComponent), LifestyleType.Singleton, false);
		TestHandlersLifestyle(typeof(TrivialComponent), LifestyleType.Thread, false);
		TestHandlersLifestyle(typeof(TrivialComponent), LifestyleType.Transient, false);

		TestHandlersLifestyleWithService(typeof(TrivialComponent), LifestyleType.Transient, false);
		TestHandlersLifestyleWithService(typeof(TrivialComponent), LifestyleType.Singleton, false);
		TestHandlersLifestyleWithService(typeof(TrivialComponent), LifestyleType.Thread, false);
		TestHandlersLifestyleWithService(typeof(TrivialComponent), LifestyleType.Transient, false);

		TestLifestyleAndSameness(typeof(PerThreadComponent), LifestyleType.Transient, true, false);
		TestLifestyleAndSameness(typeof(Components.SingletonComponent), LifestyleType.Transient, true, false);
		TestLifestyleAndSameness(typeof(TransientComponent), LifestyleType.Singleton, true, true);

		TestLifestyleWithServiceAndSameness(typeof(PerThreadComponent), LifestyleType.Transient, true, false);
		TestLifestyleWithServiceAndSameness(typeof(Components.SingletonComponent), LifestyleType.Transient, true, false);
		TestLifestyleWithServiceAndSameness(typeof(TransientComponent), LifestyleType.Singleton, true, true);
	}

	[Fact]
	public void LifestyleSetThroughAttribute()
	{
		Kernel.Register(Component.For(typeof(TransientComponent)).Named("a"));
		var handler = Kernel.GetHandler("a");
		Assert.Equal(LifestyleType.Transient, handler.ComponentModel.LifestyleType);

		Kernel.Register(Component.For(typeof(Components.SingletonComponent)).Named("b"));
		handler = Kernel.GetHandler("b");
		Assert.Equal(LifestyleType.Singleton, handler.ComponentModel.LifestyleType);

		Kernel.Register(Component.For(typeof(CustomComponent)).Named("c"));
		handler = Kernel.GetHandler("c");
		Assert.Equal(LifestyleType.Custom, handler.ComponentModel.LifestyleType);
	}

	[Fact]
	public void LifestyleSetThroughExternalConfig()
	{
		IConfiguration confignode = new MutableConfiguration("component");
		confignode.Attributes.Add("lifestyle", "transient");
		Kernel.ConfigurationStore.AddComponentConfiguration("a", confignode);
		Kernel.Register(Component.For(typeof(TrivialComponent)).Named("a"));
		var handler = Kernel.GetHandler("a");
		Assert.Equal(LifestyleType.Transient, handler.ComponentModel.LifestyleType);

		confignode = new MutableConfiguration("component");
		confignode.Attributes.Add("lifestyle", "singleton");
		Kernel.ConfigurationStore.AddComponentConfiguration("b", confignode);
		Kernel.Register(Component.For(typeof(TrivialComponent)).Named("b"));
		handler = Kernel.GetHandler("b");
		Assert.Equal(LifestyleType.Singleton, handler.ComponentModel.LifestyleType);
	}

	[Fact]
	public void Lifestyle_from_configuration_overwrites_attribute()
	{
		var confignode = new MutableConfiguration("component");
		confignode.Attributes.Add("lifestyle", "transient");
		Kernel.ConfigurationStore.AddComponentConfiguration("a", confignode);
		Kernel.Register(Component.For(typeof(Components.SingletonComponent)).Named("a"));
		var handler = Kernel.GetHandler("a");
		Assert.Equal(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
	}

	[Fact]
	public void Lifestyle_from_fluent_registration_overwrites_attribute()
	{
		Kernel.Register(Component.For<Components.SingletonComponent>().Named("a").LifeStyle.Transient);
		var handler = Kernel.GetHandler("a");
		Assert.Equal(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
	}

	[Fact]
	public void Per_dependency_tree()
	{
		Kernel.Register(
			Component.For<Root>().ExtendedProperties(ScopeRoot()),
			Component.For<Branch>(),
			Component.For<Leaf>().LifestyleCustom<CustomLifestyle_Scoped>()
		);
		var root = Kernel.Resolve<Root>();
		Assert.Same(root.Leaf, root.Branch.Leaf);
	}

	[Fact]
	public void TestPerThread()
	{
		Kernel.Register(Component.For<IComponent>().ImplementedBy(typeof(PerThreadComponent)).Named("a"));

		var handler = Kernel.GetHandler("a");

		var instance1 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;
		var instance2 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;

		Assert.NotNull(instance1);
		Assert.NotNull(instance2);

		Assert.True(instance1.Equals(instance2));
		Assert.True(instance1.ID == instance2.ID);

		var thread = new Thread(OtherThread);
		thread.Start();
		thread.Join();

		Assert.NotNull(instance3);
		Assert.True(!instance1.Equals(instance3));
		Assert.True(instance1.ID != instance3.ID);
	}

	[Fact]
	public void TestSingleton()
	{
		Kernel.Register(Component.For<IComponent>().ImplementedBy(typeof(Components.SingletonComponent)).Named("a"));

		var handler = Kernel.GetHandler("a");

		var instance1 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;
		var instance2 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;

		Assert.NotNull(instance1);
		Assert.NotNull(instance2);

		Assert.True(instance1.Equals(instance2));
		Assert.True(instance1.ID == instance2.ID);
	}

	[Fact]
	public void BoundTo_via_attribute()
	{
		Kernel.Register(
			Component.For(typeof(GenericA<>)),
			Component.For(typeof(GenericB<>)),
			Component.For<IComponent>().ImplementedBy<BoundComponent>());

		var handler = Kernel.GetHandler(typeof(IComponent));

		Assert.Equal(LifestyleType.Bound, handler.ComponentModel.LifestyleType);

		var a = Kernel.Resolve<GenericA<IComponent>>();

		Assert.Same(a.Item, a.B.Item);
	}

	[Fact]
	public void TestTransient()
	{
		Kernel.Register(Component.For<IComponent>().ImplementedBy(typeof(TransientComponent)).Named("a"));

		var handler = Kernel.GetHandler("a");

		var instance1 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;
		var instance2 = handler.Resolve(CreationContext.CreateEmpty()) as IComponent;

		Assert.NotNull(instance1);
		Assert.NotNull(instance2);

		Assert.True(!instance1.Equals(instance2));
		Assert.True(instance1.ID != instance2.ID);
	}
}