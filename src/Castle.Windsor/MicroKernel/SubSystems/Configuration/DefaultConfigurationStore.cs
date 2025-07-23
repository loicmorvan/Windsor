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

using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Configuration;
using Castle.Core.Resource;
using Castle.Windsor.MicroKernel.SubSystems.Resource;

namespace Castle.Windsor.MicroKernel.SubSystems.Configuration;

/// <summary>
///     This implementation of <see cref = "IConfigurationStore" /> does not try to obtain an external configuration by any means. Its only purpose is to serve as a base class for subclasses that might
///     obtain the configuration node from anywhere.
/// </summary>
[Serializable]
public class DefaultConfigurationStore : AbstractSubSystem, IConfigurationStore
{
	private readonly IDictionary<string, IConfiguration> _childContainers = new Dictionary<string, IConfiguration>();
	private readonly IDictionary<string, IConfiguration> _components = new Dictionary<string, IConfiguration>();
	private readonly IDictionary<string, IConfiguration> _facilities = new Dictionary<string, IConfiguration>();
	private readonly ICollection<IConfiguration> _installers = new List<IConfiguration>();
	private readonly object _syncLock = new();

	/// <summary>Adds the child container configuration.</summary>
	/// <param name = "key">The key.</param>
	/// <param name = "config">The config.</param>
	public void AddChildContainerConfiguration(string key, IConfiguration config)
	{
		lock (_syncLock)
		{
			_childContainers[key] = config;
		}
	}

	/// <summary>Associates a configuration node with a component key</summary>
	/// <param name = "key">item key</param>
	/// <param name = "config">Configuration node</param>
	public void AddComponentConfiguration(string key, IConfiguration config)
	{
		lock (_syncLock)
		{
			_components[key] = config;
		}
	}

	/// <summary>Associates a configuration node with a facility key</summary>
	/// <param name = "key">item key</param>
	/// <param name = "config">Configuration node</param>
	public void AddFacilityConfiguration(string key, IConfiguration config)
	{
		lock (_syncLock)
		{
			_facilities[key] = config;
		}
	}

	public void AddInstallerConfiguration(IConfiguration config)
	{
		lock (_syncLock)
		{
			_installers.Add(config);
		}
	}

	/// <summary>Returns the configuration node associated with the specified child container key. Should return null if no association exists.</summary>
	/// <param name = "key">item key</param>
	/// <returns></returns>
	public IConfiguration GetChildContainerConfiguration(string key)
	{
		lock (_syncLock)
		{
			IConfiguration value;
			_childContainers.TryGetValue(key, out value);
			return value;
		}
	}

	/// <summary>Returns the configuration node associated with the specified component key. Should return null if no association exists.</summary>
	/// <param name = "key">item key</param>
	/// <returns></returns>
	public IConfiguration GetComponentConfiguration(string key)
	{
		lock (_syncLock)
		{
			IConfiguration value;
			_components.TryGetValue(key, out value);
			return value;
		}
	}

	/// <summary>Returns all configuration nodes for components</summary>
	/// <returns></returns>
	public IConfiguration[] GetComponents()
	{
		lock (_syncLock)
		{
			return _components.Values.ToArray();
		}
	}

	/// <summary>Returns all configuration nodes for child containers</summary>
	/// <returns></returns>
	public IConfiguration[] GetConfigurationForChildContainers()
	{
		lock (_syncLock)
		{
			return _childContainers.Values.ToArray();
		}
	}

	/// <summary>Returns all configuration nodes for facilities</summary>
	/// <returns></returns>
	public IConfiguration[] GetFacilities()
	{
		lock (_syncLock)
		{
			return _facilities.Values.ToArray();
		}
	}

	/// <summary>Returns the configuration node associated with the specified facility key. Should return null if no association exists.</summary>
	/// <param name = "key">item key</param>
	/// <returns></returns>
	public IConfiguration GetFacilityConfiguration(string key)
	{
		lock (_syncLock)
		{
			IConfiguration value;
			_facilities.TryGetValue(key, out value);
			return value;
		}
	}

	public IConfiguration[] GetInstallers()
	{
		lock (_syncLock)
		{
			return _installers.ToArray();
		}
	}

	public IResource GetResource(string resourceUri, IResource resource)
	{
		if (resourceUri.IndexOf(Uri.SchemeDelimiter) == -1) return resource.CreateRelative(resourceUri);

		var subSystem = (IResourceSubSystem)Kernel.GetSubSystem(SubSystemConstants.ResourceKey);

		return subSystem.CreateResource(resourceUri, resource.FileBasePath);
	}

	public override void Terminate()
	{
	}
}