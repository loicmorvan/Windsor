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
using Castle.MicroKernel;

namespace Castle.Core;

/// <summary>
///     Indicates that the target components wants instance lifetime and reuse scope to be bound to another component
///     further up the object graph.
///     Good scenario for this would be unit of work bound to a presenter in a two tier MVP application.
///     The <see cref="ScopeRootBinderType" /> attribute must point to a type
///     having default accessible constructor and public method matching signature of
///     <code>Func&lt;IHandler[], IHandler&gt;</code> delegate.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class BoundToAttribute : LifestyleAttribute
{
	/// <summary>
	///     Initializes a new instance of the <see cref="BoundToAttribute" /> class.
	/// </summary>
	/// <param name="scopeRootBinderType">
	///     type having default accessible constructor and public method matching signature of
	///     <code>Func&lt;IHandler[], IHandler&gt;</code> delegate. The method will be used to pick
	///     <see
	///         cref="IHandler" />
	///     of the component current instance should be bound to.
	/// </param>
	public BoundToAttribute(Type scopeRootBinderType)
		: base(LifestyleType.Bound)
	{
		ScopeRootBinderType = scopeRootBinderType;
	}

	/// <summary>
	///     type having default accessible constructor and public method matching signature of
	///     <code>Func&lt;IHandler[], IHandler&gt;</code> delegate. The method will be used to pick
	///     <see
	///         cref="IHandler" />
	///     of the component current instance should be bound to.
	/// </summary>
	public Type ScopeRootBinderType { get; private set; }
}