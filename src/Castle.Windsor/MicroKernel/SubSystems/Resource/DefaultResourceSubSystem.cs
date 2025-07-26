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
using Castle.Core.Resource;

namespace Castle.Windsor.MicroKernel.SubSystems.Resource;

/// <summary>Pendent</summary>
public sealed class DefaultResourceSubSystem : AbstractSubSystem, IResourceSubSystem
{
	private readonly List<IResourceFactory> _resourceFactories = new();

	public DefaultResourceSubSystem()
	{
		InitDefaultResourceFactories();
	}

	public IResource CreateResource(string resource)
	{
		ArgumentNullException.ThrowIfNull(resource);

		return CreateResource(new CustomUri(resource));
	}

	public IResource CreateResource(string resource, string basePath)
	{
		ArgumentNullException.ThrowIfNull(resource);

		return CreateResource(new CustomUri(resource), basePath);
	}

	public IResource CreateResource(CustomUri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);

		foreach (var resFactory in _resourceFactories)
			if (resFactory.Accept(uri))
				return resFactory.Create(uri);

		throw new KernelException("No Resource factory was able to " +
		                          "deal with Uri " + uri);
	}

	public IResource CreateResource(CustomUri uri, string basePath)
	{
		ArgumentNullException.ThrowIfNull(uri);
		ArgumentNullException.ThrowIfNull(basePath);

		foreach (var resFactory in _resourceFactories)
			if (resFactory.Accept(uri))
				return resFactory.Create(uri, basePath);

		throw new KernelException("No Resource factory was able to " +
		                          "deal with Uri " + uri);
	}

	public void RegisterResourceFactory(IResourceFactory resourceFactory)
	{
		ArgumentNullException.ThrowIfNull(resourceFactory);

		_resourceFactories.Add(resourceFactory);
	}

	private void InitDefaultResourceFactories()
	{
		RegisterResourceFactory(new AssemblyResourceFactory());
		RegisterResourceFactory(new UncResourceFactory());
		RegisterResourceFactory(new FileResourceFactory());
	}
}