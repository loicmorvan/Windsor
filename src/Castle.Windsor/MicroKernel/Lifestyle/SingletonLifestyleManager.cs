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
using Castle.Core.Internal;
using Castle.MicroKernel.ComponentActivator;
using Castle.MicroKernel.Context;

namespace Castle.MicroKernel.Lifestyle;

/// <summary>
///     Only one instance is created first time an instance of the component is requested, and it is then reused for all
///     subseque.
/// </summary>
[Serializable]
public class SingletonLifestyleManager : AbstractLifestyleManager, IContextLifestyleManager
{
	private readonly ThreadSafeInit _init = new();
	private Burden _cachedBurden;

	public object GetContextInstance(CreationContext context)
	{
		return context.GetContextualProperty(DefaultComponentActivator.InstanceStash);
	}

	public override void Dispose()
	{
		var localInstance = _cachedBurden;
		if (localInstance != null)
		{
			localInstance.Release();
			_cachedBurden = null;
		}
	}

	public override object Resolve(CreationContext context, IReleasePolicy releasePolicy)
	{
		// 1. read from cache
		if (_cachedBurden != null) return _cachedBurden.Instance;
		var initializing = false;
		try
		{
			initializing = _init.ExecuteThreadSafeOnce();
			if (_cachedBurden != null) return _cachedBurden.Instance;
			var burden = CreateInstance(context, true);
			_cachedBurden = burden;
			Track(burden, releasePolicy);
			return burden.Instance;
		}
		finally
		{
			if (initializing) _init.EndThreadSafeOnceSection();
		}
	}
}