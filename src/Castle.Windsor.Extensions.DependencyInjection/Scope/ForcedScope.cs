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

using System;

namespace Castle.Windsor.Extensions.DependencyInjection.Scope
{
	/// <summary>
	///     Forces a specific <see name="ExtensionContainerScope" /> for 'using' block. In .NET scope is tied to an
	///     instance of <see name="System.IServiceProvider" /> not a thread or async context
	/// </summary>
	internal class ForcedScope : IDisposable
	{
		private readonly ExtensionContainerScopeBase _previousScope;
		private readonly ExtensionContainerScopeBase _scope;

		internal ForcedScope(ExtensionContainerScopeBase scope)
		{
			_previousScope = ExtensionContainerScopeCache.Current;
			_scope = scope;
			ExtensionContainerScopeCache.Current = scope;
		}

		public void Dispose()
		{
			if (ExtensionContainerScopeCache.Current != _scope) return;
			ExtensionContainerScopeCache.Current = _previousScope;
		}
	}
}