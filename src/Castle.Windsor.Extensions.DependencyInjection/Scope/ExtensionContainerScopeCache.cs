﻿// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
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
using System.Threading;

namespace Castle.Windsor.Extensions.DependencyInjection.Scope;

internal static class ExtensionContainerScopeCache
{
	private static readonly AsyncLocal<ExtensionContainerScopeBase> CurrentAsyncLocal = new();

	/// <summary>
	///     Current scope for the thread. Initial scope will be set when calling BeginRootScope from a
	///     ExtensionContainerRootScope instance.
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown when there is no scope available.</exception>
	internal static ExtensionContainerScopeBase Current
	{
		get => CurrentAsyncLocal.Value ?? throw new InvalidOperationException("No scope available");
		set => CurrentAsyncLocal.Value = value;
	}
}