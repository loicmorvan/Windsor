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

using System.Collections.Generic;
using System.Linq;

using Castle.Core.Configuration;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;

public class KernelEvents_DependencyResolving_TestCase : AbstractContainerTestCase
{
	private ComponentModel expectedClient;
	private List<DependencyModel> expectedModels;

	protected override void AfterContainerCreated()
	{
		Kernel.DependencyResolving += AssertEvent;
	}

	private void AssertEvent(ComponentModel client, DependencyModel model, object dependency)
	{
		Assert.Equal(expectedClient, client);
		Assert.Contains(model, expectedModels);
		Assert.NotNull(dependency);
	}

	[Fact]
	public void ResolvingConcreteClassThroughConstructor()
	{
		Kernel.Register(Component.For(typeof(DefaultSpamServiceWithConstructor)).Named("spamservice"));
		Kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
		Kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

		var mailservice = Kernel.Resolve<DefaultMailSenderService>("mailsender");
		var templateengine = Kernel.Resolve<DefaultTemplateEngine>("templateengine");

		Assert.NotNull(mailservice);
		Assert.NotNull(templateengine);

		expectedClient = Kernel.GetHandler("spamservice").ComponentModel;
		expectedModels =
			new List<DependencyModel>(
				Kernel.GetHandler("spamservice").ComponentModel.Constructors.Single().Dependencies);

		var spamservice =
			Kernel.Resolve<DefaultSpamServiceWithConstructor>("spamservice");

		Assert.NotNull(spamservice);
	}

	[Fact]
	public void ResolvingConcreteClassThroughProperties()
	{
		Kernel.Register(Component.For<DefaultSpamService>().Named("spamservice"));
		Kernel.Register(Component.For<DefaultMailSenderService>().Named("mailsender"));
		Kernel.Register(Component.For<DefaultTemplateEngine>().Named("templateengine"));

		var mailservice = Kernel.Resolve<DefaultMailSenderService>("mailsender");
		var templateengine = Kernel.Resolve<DefaultTemplateEngine>("templateengine");

		Assert.NotNull(mailservice);
		Assert.NotNull(templateengine);

		expectedClient = Kernel.GetHandler("spamservice").ComponentModel;
		expectedModels = new List<DependencyModel>();
		foreach (var prop in Kernel.GetHandler("spamservice").ComponentModel.Properties) expectedModels.Add(prop.Dependency);

		var spamservice = Kernel.Resolve<DefaultSpamService>("spamservice");

		Assert.NotNull(spamservice);
	}

	[Fact]
	public void ResolvingPrimitivesThroughProperties()
	{
		var config = new MutableConfiguration("component");

		var parameters = new MutableConfiguration("parameters");
		config.Children.Add(parameters);

		parameters.Children.Add(new MutableConfiguration("name", "hammett"));
		parameters.Children.Add(new MutableConfiguration("address", "something"));
		parameters.Children.Add(new MutableConfiguration("age", "25"));

		Kernel.ConfigurationStore.AddComponentConfiguration("customer", config);

		Kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerImpl)).Named("customer"));

		expectedClient = Kernel.GetHandler("customer").ComponentModel;
		expectedModels = new List<DependencyModel>();
		foreach (var prop in Kernel.GetHandler("customer").ComponentModel.Properties) expectedModels.Add(prop.Dependency);

		var customer = Kernel.Resolve<ICustomer>("customer");

		Assert.NotNull(customer);
	}
}