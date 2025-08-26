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

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Castle.Windsor.Core;

namespace Castle.Windsor.MicroKernel.LifecycleConcerns;

/// <summary>Lifetime concern that works for components that don't have their actual type determined upfront</summary>
[Serializable]
public abstract class LateBoundConcerns<TConcern>
{
    private Dictionary<Type, TConcern>? _concerns;
    private ConcurrentDictionary<Type, List<TConcern>>? _concernsCache;

    public bool HasConcerns => _concerns != null;

    public void AddConcern<TForType>(TConcern lifecycleConcern)
    {
        if (_concerns == null)
        {
            _concerns = new Dictionary<Type, TConcern>(2);
            _concernsCache = new ConcurrentDictionary<Type, List<TConcern>>(2, 2);
        }

        _concerns.Add(typeof(TForType), lifecycleConcern);
    }

    public abstract void Apply(ComponentModel model, object component);

    private List<TConcern> BuildConcernCache(Type type)
    {
        Debug.Assert(_concerns != null, nameof(_concerns) + " != null");
        var componentConcerns = new List<TConcern>(_concerns.Count);
        componentConcerns.AddRange(from concern in _concerns
            where concern.Key.GetTypeInfo().IsAssignableFrom(type)
            select concern.Value);

        return componentConcerns;
    }

    protected List<TConcern> GetComponentConcerns(Type type)
    {
        Debug.Assert(_concernsCache != null, nameof(_concernsCache) + " != null");
        return _concernsCache.GetOrAdd(type, BuildConcernCache);
    }
}