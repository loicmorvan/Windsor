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

using Castle.Windsor.Core;

namespace Castle.Windsor.MicroKernel.Lifestyle.Scoped;

/// <remarks>This class is not thread safe like CallContextLifetimeScope.</remarks>
public class DefaultLifetimeScope(IScopeCache scopeCache = null, Action<Burden> onAfterCreated = null)
    : ILifetimeScope
{
    private static readonly Action<Burden> EmptyOnAfterCreated = delegate { };
    private readonly IScopeCache _scopeCache = scopeCache ?? new ScopeCache();

    internal Action<Burden> OnAfterCreated { get; set; } = onAfterCreated ?? EmptyOnAfterCreated;

    public void Dispose()
    {
        if (_scopeCache is IDisposable disposableCache)
        {
            disposableCache.Dispose();
        }
    }

    public Burden GetCachedInstance(ComponentModel model, ScopedInstanceActivationCallback createInstance)
    {
        var burden = _scopeCache[model];
        if (burden == null)
        {
            _scopeCache[model] = burden = createInstance(OnAfterCreated);
        }

        return burden;
    }
}