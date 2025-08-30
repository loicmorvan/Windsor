// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
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
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Lifestyle.Scoped;

namespace Castle.Windsor.Extensions.DependencyInjection.Scope;

internal abstract class ExtensionContainerScopeBase : ILifetimeScope
{
    public const string TransientMarker = "Transient";
    private readonly ScopeCache _scopeCache = new();

    internal virtual ExtensionContainerScopeBase? RootScope { get; set; }

    public virtual void Dispose()
    {
        if (_scopeCache is IDisposable disposableCache)
        {
            disposableCache.Dispose();
        }
    }

    public Burden GetCachedInstance(ComponentModel? model, ScopedInstanceActivationCallback createInstance)
    {
        lock (_scopeCache)
        {
            // Add transient's burden to scope so it gets released
            Debug.Assert(model?.Configuration != null);
            if (model.Configuration.Attributes.Get(TransientMarker) == bool.TrueString)
            {
                var transientBurden = createInstance(_ => { });
                _scopeCache[transientBurden] = transientBurden;
                return transientBurden;
            }

            var scopedBurden = _scopeCache[model];
            if (scopedBurden != null)
            {
                return scopedBurden;
            }

            scopedBurden = createInstance(_ => { });
            _scopeCache[model] = scopedBurden;
            return scopedBurden;
        }
    }
}