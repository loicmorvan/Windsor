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

using System;
using Castle.Core.Internal;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle.Pool;
using Castle.MicroKernel.Registration;

namespace Castle.MicroKernel.Lifestyle;

/// <summary>
///     Manages a pool of objects.
/// </summary>
[Serializable]
public class PoolableLifestyleManager(int initialSize, int maxSize) : AbstractLifestyleManager
{
	private static readonly object PoolFactoryLock = new();
	private readonly ThreadSafeInit _init = new();
	private readonly int _initialSize = initialSize;
	private readonly int _maxSize = maxSize;
	private IPool _pool;

	protected IPool Pool
	{
		get
		{
			if (_pool != null) return _pool;
			var initializing = false;
			try
			{
				initializing = _init.ExecuteThreadSafeOnce();

				if (_pool == null) _pool = CreatePool(_initialSize, _maxSize);
				return _pool;
			}
			finally
			{
				if (initializing) _init.EndThreadSafeOnceSection();
			}
		}
	}

	public override void Dispose()
	{
		if (_pool != null) _pool.Dispose();
	}

	public override bool Release(object instance)
	{
		if (_pool != null) return _pool.Release(instance);
		return false;
	}

	public override object Resolve(CreationContext context, IReleasePolicy releasePolicy)
	{
		return Pool.Request(context, c => PoolCreationCallback(c, releasePolicy));
	}

	protected IPool CreatePool(int initialSize, int maxSize)
	{
		if (!Kernel.HasComponent(typeof(IPoolFactory)))
			lock (PoolFactoryLock)
			{
				if (!Kernel.HasComponent(typeof(IPoolFactory)))
					Kernel.Register(Component.For<IPoolFactory>()
						.ImplementedBy<DefaultPoolFactory>()
						.NamedAutomatically("castle.internal-pool-factory"));
			}

		var factory = Kernel.Resolve<IPoolFactory>();
		return factory.Create(initialSize, maxSize, ComponentActivator);
	}

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