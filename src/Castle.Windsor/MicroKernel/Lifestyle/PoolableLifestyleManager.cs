// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel.Context;
using Castle.Windsor.MicroKernel.Lifestyle.Pool;
using Castle.Windsor.MicroKernel.Registration;
using JetBrains.Annotations;

namespace Castle.Windsor.MicroKernel.Lifestyle;

/// <summary>Manages a pool of objects.</summary>
[Serializable]
public class PoolableLifestyleManager(int initialSize, int maxSize) : AbstractLifestyleManager
{
    private static readonly Lock PoolFactoryLock = new();
    private readonly ThreadSafeInit _init = new();
    private readonly int _initialSize = initialSize;
    private readonly int _maxSize = maxSize;
    private IPool? _pool;

    protected IPool Pool
    {
        get
        {
            if (_pool != null)
            {
                return _pool;
            }

            var initializing = false;
            try
            {
                initializing = _init.ExecuteThreadSafeOnce();

                return _pool ??= CreatePool(_initialSize, _maxSize);
            }
            finally
            {
                if (initializing)
                {
                    _init.EndThreadSafeOnceSection();
                }
            }
        }
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        
        _pool?.Dispose();
    }

    public override bool Release(object instance)
    {
        return _pool != null && _pool.Release(instance);
    }

    public override object Resolve(CreationContext context, IReleasePolicy releasePolicy)
    {
        return Pool.Request(context, c => PoolCreationCallback(c, releasePolicy));
    }

    protected IPool CreatePool(int initialSize, int maxSize)
    {
        Debug.Assert(Kernel != null, nameof(Kernel) + " != null");
        if (!Kernel.HasComponent(typeof(IPoolFactory)))
        {
            lock (PoolFactoryLock)
            {
                if (!Kernel.HasComponent(typeof(IPoolFactory)))
                {
                    Kernel.Register(Component.For<IPoolFactory>()
                        .ImplementedBy<DefaultPoolFactory>()
                        .NamedAutomatically("castle.internal-pool-factory"));
                }
            }
        }

        var factory = Kernel.Resolve<IPoolFactory>();
        return factory.Create(initialSize, maxSize, ComponentActivator);
    }

    [PublicAPI]
    protected virtual Burden PoolCreationCallback(CreationContext context, IReleasePolicy releasePolicy)
    {
        var burden = base.CreateInstance(context, false);
        Track(burden, releasePolicy);
        return burden;
    }

    protected override void Track(Burden burden, IReleasePolicy releasePolicy)
    {
        burden.RequiresDecommission = true;
        releasePolicy.Track(burden.Instance, burden);
    }
}