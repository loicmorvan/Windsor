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

using Castle.Core.Configuration;
using Castle.Core.Resource;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.SubSystems.Configuration;

namespace Castle.Windsor.Windsor.Installer;

internal class PartialConfigurationStore : IConfigurationStore, IDisposable
{
    private readonly IConfigurationStore _inner;
    private readonly DefaultConfigurationStore _partial;

    public PartialConfigurationStore(IKernelInternal kernel)
    {
        _inner = kernel.ConfigurationStore ??
                 throw new InvalidOperationException("The kernel does not have a configuration store.");
        _partial = new DefaultConfigurationStore();
        _partial.Init(kernel);
    }

    public void AddChildContainerConfiguration(string name, IConfiguration config)
    {
        _inner.AddChildContainerConfiguration(name, config);
        _partial.AddChildContainerConfiguration(name, config);
    }

    public void AddComponentConfiguration(string key, IConfiguration config)
    {
        _inner.AddComponentConfiguration(key, config);
        _partial.AddComponentConfiguration(key, config);
    }

    public void AddFacilityConfiguration(string key, IConfiguration config)
    {
        _inner.AddFacilityConfiguration(key, config);
        _partial.AddFacilityConfiguration(key, config);
    }

    public void AddInstallerConfiguration(IConfiguration config)
    {
        _inner.AddInstallerConfiguration(config);
        _partial.AddInstallerConfiguration(config);
    }

    public IConfiguration? GetChildContainerConfiguration(string key)
    {
        return _partial.GetChildContainerConfiguration(key);
    }

    public IConfiguration? GetComponentConfiguration(string key)
    {
        return _partial.GetComponentConfiguration(key);
    }

    public IConfiguration[] GetComponents()
    {
        return _partial.GetComponents();
    }

    public IConfiguration[] GetConfigurationForChildContainers()
    {
        return _partial.GetConfigurationForChildContainers();
    }

    public IConfiguration[] GetFacilities()
    {
        return _partial.GetFacilities();
    }

    public IConfiguration? GetFacilityConfiguration(string key)
    {
        return _partial.GetFacilityConfiguration(key);
    }

    public IConfiguration[] GetInstallers()
    {
        return _partial.GetInstallers();
    }

    public IResource GetResource(string resourceUri, IResource resource)
    {
        return _inner.GetResource(resourceUri, resource);
    }

    public void Init(IKernelInternal kernel)
    {
        _partial.Init(kernel);
    }

    public void Terminate()
    {
        _partial.Terminate();
    }

    public void Dispose()
    {
        Terminate();
    }
}