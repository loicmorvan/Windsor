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

namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>Represents a service override.</summary>
public class ServiceOverride
{
	internal ServiceOverride(object dependencyKey, object value) : this(dependencyKey, value, null)
	{
	}

	internal ServiceOverride(object dependencyKey, object value, Type type)
	{
		DependencyKey = dependencyKey;
		Value = value;
		Type = type;
	}

	public object DependencyKey { get; private set; }

	/// <summary>Gets the optional value type specifier.</summary>
	public Type Type { get; private set; }

	public object Value { get; private set; }

	/// <summary>Creates a <see cref = "ServiceOverrideKey" /> with key.</summary>
	/// <param name = "key">The service override key.</param>
	/// <returns>The new <see cref = "ServiceOverrideKey" /></returns>
	public static ServiceOverrideKey ForKey(string key)
	{
		return new ServiceOverrideKey(key);
	}

	/// <summary>Creates a <see cref = "ServiceOverrideKey" /> with key.</summary>
	/// <param name = "key">The service override key.</param>
	/// <returns>The new <see cref = "ServiceOverrideKey" /></returns>
	public static ServiceOverrideKey ForKey(Type key)
	{
		return new ServiceOverrideKey(key);
	}

	/// <summary>Creates a <see cref = "ServiceOverrideKey" /> with key.</summary>
	/// <typeparam name = "TKey">The service override key.</typeparam>
	/// <returns>The new <see cref = "ServiceOverrideKey" /></returns>
	public static ServiceOverrideKey ForKey<TKey>()
	{
		return new ServiceOverrideKey(typeof(TKey));
	}

	/// <summary>Implicitly converts service override to dependency. This is a API trick to keep the API clean and focused.</summary>
	/// <param name = "item"></param>
	/// <returns></returns>
	public static implicit operator Dependency(ServiceOverride item)
	{
		return item == null ? null : new Dependency(item);
	}
}