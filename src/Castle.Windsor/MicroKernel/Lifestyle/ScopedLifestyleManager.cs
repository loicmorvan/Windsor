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

using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel.Context;
using Castle.Windsor.MicroKernel.Lifestyle.Scoped;
using JetBrains.Annotations;

namespace Castle.Windsor.MicroKernel.Lifestyle;

public class ScopedLifestyleManager(IScopeAccessor accessor) : AbstractLifestyleManager
{
    private IScopeAccessor? _accessor = accessor;

    [PublicAPI]
    public ScopedLifestyleManager()
        : this(new LifetimeScopeAccessor())
    {
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        
        var scope = Interlocked.Exchange(ref _accessor, null);
        scope?.Dispose();
    }

    public override object Resolve(CreationContext context, IReleasePolicy releasePolicy)
    {
        var scope = GetScope(context);
        var burden = scope.GetCachedInstance(Model, afterCreated =>
        {
            var localBurden = base.CreateInstance(context, true);
            afterCreated(localBurden);
            Track(localBurden, releasePolicy);
            return localBurden;
        });
        return burden.Instance;
    }

    private ILifetimeScope GetScope(CreationContext context)
    {
        var localScope = _accessor;
        ObjectDisposedException.ThrowIf(localScope is null, this);

        var scope = localScope.GetScope(context);
        if (scope == null)
        {
            throw new ComponentResolutionException(
                $"Could not obtain scope for component {Model?.Name ?? "null"}. This is most likely either a bug in custom {typeof(IScopeAccessor).ToCSharpString()} or you're trying to access scoped component outside of the scope (like a per-web-request component outside of web request etc)",
                Model);
        }

        return scope;
    }
}