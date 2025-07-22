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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Internal;

namespace Castle.Windsor.MicroKernel.Lifestyle.Scoped;

#if FEATURE_REMOTING
	using System.Runtime.Remoting.Messaging;
#endif
#if !FEATURE_REMOTING
using System.Threading;
#endif
using Lock = Lock;

/// <summary>Provides explicit lifetime scoping within logical path of execution. Used for types with <see cref = "LifestyleType.Scoped" />.</summary>
/// <remarks>
///     The scope is passed on to child threads, including ThreadPool threads. The capability is limited to a single AppDomain and should be used cautiously as calls to <see cref = "Dispose" /> may occur
///     while the child thread is still executing, which in turn may lead to subtle threading bugs.
/// </remarks>
public class CallContextLifetimeScope : ILifetimeScope
{
	private static readonly ConcurrentDictionary<Guid, CallContextLifetimeScope> AllScopes = new();

#if FEATURE_REMOTING
		private static readonly string callContextKey = "castle.lifetime-scope-" + AppDomain.CurrentDomain.Id.ToString(CultureInfo.InvariantCulture);
#else
	private static readonly AsyncLocal<Guid> AsyncLocal = new();
#endif

	private readonly Guid _contextId;
	private readonly CallContextLifetimeScope _parentScope;
	private readonly Lock _lock = Lock.Create();
	private ScopeCache _cache = new();

	public CallContextLifetimeScope()
	{
		_contextId = Guid.NewGuid();
		_parentScope = ObtainCurrentScope();

		var added = AllScopes.TryAdd(_contextId, this);
		Debug.Assert(added);
		SetCurrentScope(this);
	}

	[SecuritySafeCritical]
	public void Dispose()
	{
		using (var token = _lock.ForReadingUpgradeable())
		{
			// Dispose the burden cache
			if (_cache == null) return;
			token.Upgrade();
			_cache.Dispose();
			_cache = null;

			// Restore the parent scope (if inside one)
			if (_parentScope != null)
			{
				SetCurrentScope(_parentScope);
			}
			else
			{
#if FEATURE_REMOTING
					CallContext.FreeNamedDataSlot(callContextKey);
#endif
			}
		}

		AllScopes.TryRemove(_contextId, out _);
	}

	public Burden GetCachedInstance(ComponentModel model, ScopedInstanceActivationCallback createInstance)
	{
		using var token = _lock.ForReadingUpgradeable();
		var burden = _cache[model];
		if (burden == null)
		{
			token.Upgrade();

			burden = createInstance(delegate { });
			_cache[model] = burden;
		}

		return burden;
	}

	[SecuritySafeCritical]
	private static void SetCurrentScope(CallContextLifetimeScope lifetimeScope)
	{
#if FEATURE_REMOTING
			CallContext.LogicalSetData(callContextKey, lifetimeScope.contextId);
#else
		AsyncLocal.Value = lifetimeScope._contextId;
#endif
	}

	[SecuritySafeCritical]
	public static CallContextLifetimeScope ObtainCurrentScope()
	{
#if FEATURE_REMOTING
		object	scopeKey = CallContext.LogicalGetData(callContextKey);
#else
		object scopeKey = AsyncLocal.Value;
#endif
		AllScopes.TryGetValue((Guid)scopeKey, out var scope);
		return scope;
	}
}