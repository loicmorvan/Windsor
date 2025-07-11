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

namespace Castle.Core;

/// <summary>
///     Indicates that the target components wants a
///     pooled lifestyle.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class PooledAttribute : LifestyleAttribute
{
	private const int DefaultInitialPoolSize = 5;
	private const int DefaultMaxPoolSize = 15;

	/// <summary>
	///     Initializes a new instance of the <see cref="PooledAttribute" /> class
	///     using the default initial pool size (5) and the max pool size (15).
	/// </summary>
	public PooledAttribute() : this(DefaultInitialPoolSize, DefaultMaxPoolSize)
	{
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="PooledAttribute" /> class.
	/// </summary>
	/// <param name="initialPoolSize">Initial size of the pool.</param>
	/// <param name="maxPoolSize">Max pool size.</param>
	public PooledAttribute(int initialPoolSize, int maxPoolSize) : base(LifestyleType.Pooled)
	{
		InitialPoolSize = initialPoolSize;
		MaxPoolSize = maxPoolSize;
	}

	/// <summary>
	///     Gets the initial size of the pool.
	/// </summary>
	/// <value>The initial size of the pool.</value>
	public int InitialPoolSize { get; }

	/// <summary>
	///     Gets the maximum pool size.
	/// </summary>
	/// <value>The size of the max pool.</value>
	public int MaxPoolSize { get; }
}