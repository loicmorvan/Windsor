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
using System.Collections.Generic;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel.ModelBuilder.Inspectors;
using Castle.Windsor.MicroKernel.SubSystems.Conversion;

namespace Castle.Windsor.MicroKernel.ModelBuilder;

/// <summary>Summary description for DefaultComponentModelBuilder.</summary>
[Serializable]
public sealed class DefaultComponentModelBuilder : IComponentModelBuilder
{
	private readonly List<IContributeComponentModelConstruction> _contributors = new();
	private readonly IKernel _kernel;

	/// <summary>Initializes a new instance of the <see cref = "DefaultComponentModelBuilder" /> class.</summary>
	/// <param name = "kernel">The kernel.</param>
	public DefaultComponentModelBuilder(IKernel kernel)
	{
		_kernel = kernel;
		InitializeContributors();
	}

	/// <summary>Gets the contributors.</summary>
	/// <value>The contributors.</value>
	public IContributeComponentModelConstruction[] Contributors => _contributors.ToArray();

	/// <summary>
	///     "To give or supply in common with others; give to a common fund or for a common purpose". The contributor should inspect the component, or even the configuration associated with the component, to
	///     add or change information in the model that can be used later.
	/// </summary>
	/// <param name = "contributor"></param>
	public void AddContributor(IContributeComponentModelConstruction contributor)
	{
		_contributors.Add(contributor);
	}

	/// <summary>Constructs a new ComponentModel by invoking the registered contributors.</summary>
	public ComponentModel BuildModel(ComponentName name, Type[] services, Type classType, Arguments extendedProperties)
	{
		var model = new ComponentModel(name, services, classType, extendedProperties);
		_contributors.ForEach(c => c.ProcessModel(_kernel, model));

		return model;
	}

	public ComponentModel BuildModel(IComponentModelDescriptor[] customContributors)
	{
		var model = new ComponentModel();
		customContributors.ForEach(c => c.BuildComponentModel(_kernel, model));

		_contributors.ForEach(c => c.ProcessModel(_kernel, model));

		var metaDescriptors = default(ICollection<IMetaComponentModelDescriptor>);
		customContributors.ForEach(c =>
		{
			c.ConfigureComponentModel(_kernel, model);
			if (c is IMetaComponentModelDescriptor meta)
			{
				metaDescriptors ??= model.GetMetaDescriptors(true);
				metaDescriptors.Add(meta);
			}
		});
		return model;
	}

	/// <summary>Removes the specified contributor</summary>
	/// <param name = "contributor"></param>
	public void RemoveContributor(IContributeComponentModelConstruction contributor)
	{
		_contributors.Remove(contributor);
	}

	/// <summary>Initializes the default contributors.</summary>
	private void InitializeContributors()
	{
		var conversionManager = _kernel.GetConversionManager();
		AddContributor(new GenericInspector());
		AddContributor(new ConfigurationModelInspector());
		AddContributor(new ConfigurationParametersInspector());
		AddContributor(new LifestyleModelInspector(conversionManager));
		AddContributor(new ConstructorDependenciesModelInspector());
		AddContributor(new PropertiesDependenciesModelInspector(conversionManager));
		AddContributor(new LifecycleModelInspector());
		AddContributor(new InterceptorInspector());
		AddContributor(new MixinInspector());
		AddContributor(new ComponentActivatorInspector(conversionManager));
		AddContributor(new ComponentProxyInspector(conversionManager));
	}
}