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

using System.Diagnostics;
using Castle.Core;
using Castle.Windsor.MicroKernel;

namespace Castle.Windsor.Windsor;

/// <summary>
///     Implementation of <see cref="IServiceProvider" /> and <see cref="IServiceProviderEx" /> that uses a
///     <see
///         cref="IWindsorContainer" />
///     or <see cref="IKernel" /> as its component's source.
/// </summary>
public class WindsorServiceProvider : IServiceProviderEx
{
    private readonly IKernelInternal? _kernel;

    public WindsorServiceProvider(IWindsorContainer container)
    {
        _kernel = container.Kernel as IKernelInternal;
        if (_kernel == null)
        {
            throw new ArgumentException($"The kernel must implement {typeof(IKernelInternal)}");
        }
    }

    // ReSharper disable once UnusedMember.Global
    public IKernel? Kernel => _kernel;

    public object? GetService(Type serviceType)
    {
        Debug.Assert(_kernel != null, nameof(_kernel) + " != null");
        return _kernel.LoadHandlerByType(null, serviceType, null) != null
            ? _kernel.Resolve(serviceType)
            : null;
    }

    public T GetService<T>() where T : class
    {
        return (T)GetService(typeof(T));
    }
}