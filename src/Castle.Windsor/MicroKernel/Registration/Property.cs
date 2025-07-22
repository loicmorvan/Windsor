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
using Castle.Windsor.Core;

namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>Represents a key/value pair.</summary>
public class Property
{
	public Property(object key, object value)
	{
		Key = key;
		Value = value;
	}

	/// <summary>Gets the property key.</summary>
	public object Key { get; }

	/// <summary>Gets the property value.</summary>
	public object Value { get; }

	/// <summary>Create a <see cref = "PropertyKey" /> with key.</summary>
	/// <param name = "key">The property key.</param>
	/// <returns>The new <see cref = "PropertyKey" /></returns>
	public static PropertyKey ForKey(string key)
	{
		return new PropertyKey(key);
	}

	/// <summary>Create a <see cref = "PropertyKey" /> with key.</summary>
	/// <param name = "key">The property key.</param>
	/// <returns>The new <see cref = "PropertyKey" /></returns>
	public static PropertyKey ForKey(Type key)
	{
		return new PropertyKey(key);
	}

	/// <summary>Create a <see cref = "PropertyKey" /> with key.</summary>
	/// <param key = "key">The property key.</param>
	/// <returns>The new <see cref = "PropertyKey" /></returns>
	public static PropertyKey ForKey<TKey>()
	{
		return new PropertyKey(typeof(TKey));
	}

	public static implicit operator Dependency(Property item)
	{
		return item == null ? null : new Dependency(item);
	}
}

/// <summary>Represents a property key.</summary>
public class PropertyKey
{
	internal PropertyKey(object key)
	{
		Key = key;
	}

	/// <summary>The property key key.</summary>
	public object Key { get; }

	/// <summary>Builds the <see cref = "Property" /> with key/value.</summary>
	/// <param name = "value">The property value.</param>
	/// <returns>The new <see cref = "Property" /></returns>
	public Property Eq(object value)
	{
		return new Property(Key, value);
	}

	/// <summary>
	///     Builds a service override using other component registered with given <paramref name = "componentName" /> as value for dependency with given
	///     <see
	///         cref = "Key" />
	///     .
	/// </summary>
	/// <param name = "componentName"></param>
	/// <returns></returns>
	public ServiceOverride Is(string componentName)
	{
		return GetServiceOverrideKey().Eq(componentName);
	}

	/// <summary>
	///     Builds a service override using other component registered with given <paramref name = "componentImplementation" /> and no explicit name, as value for dependency with given
	///     <see
	///         cref = "Key" />
	///     .
	/// </summary>
	/// <returns></returns>
	public ServiceOverride Is(Type componentImplementation)
	{
		ArgumentNullException.ThrowIfNull(componentImplementation);
		return GetServiceOverrideKey().Eq(ComponentName.DefaultNameFor(componentImplementation));
	}

	/// <summary>
	///     Builds a service override using other component registered with given
	///     <typeparam name = "TComponentImplementation" />
	///     and no explicit name, as value for dependency with given
	///     <see
	///         cref = "Key" />
	///     .
	/// </summary>
	/// <returns></returns>
	public ServiceOverride Is<TComponentImplementation>()
	{
		return Is(typeof(TComponentImplementation));
	}

	private ServiceOverrideKey GetServiceOverrideKey()
	{
		if (Key is Type) return ServiceOverride.ForKey((Type)Key);
		return ServiceOverride.ForKey((string)Key);
	}
}